using JobTrackrAPI.Data;
using JobTrackrAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(AppDbContext context, TokenService tokenService, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _tokenService = tokenService;
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    private async Task<(string accessToken, string refreshToken)> IssueTokensAsync(AppUser user)
    {
        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.CreateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        await _context.SaveChangesAsync();

        return (accessToken, refreshToken);
    }

    public record GoogleLoginRequest(string AccessToken);
    private record GoogleProfile(string Sub, string Email, string Name);

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request)
    {
        var http = _httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);

        var profileResponse = await http.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
        if (!profileResponse.IsSuccessStatusCode) return Unauthorized("Invalid Google token.");

        var profile = JsonSerializer.Deserialize<GoogleProfile>(
            await profileResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Provider == "Google" && u.ProviderId == profile.Sub);
        if (user == null)
        {
            user = new AppUser { Email = profile.Email, Name = profile.Name, Provider = "Google", ProviderId = profile.Sub };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var (accessToken, refreshToken) = await IssueTokensAsync(user);
        return Ok(new { accessToken, refreshToken });
    }

    public record GitHubLoginRequest(string Code);
    private record GitHubProfile(int Id, string Login, string? Name, string? Email);

    [HttpPost("github")]
    public async Task<IActionResult> GitHubLogin(GitHubLoginRequest request)
    {
        var http = _httpClientFactory.CreateClient();

        var tokenResponse = await http.PostAsync("https://github.com/login/oauth/access_token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _config["GitHub:ClientId"]!,
                ["client_secret"] = _config["GitHub:ClientSecret"]!,
                ["code"] = request.Code
            }));

        var query = System.Web.HttpUtility.ParseQueryString(await tokenResponse.Content.ReadAsStringAsync());
        var githubAccessToken = query["access_token"];
        if (string.IsNullOrEmpty(githubAccessToken)) return Unauthorized("GitHub authentication failed.");

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubAccessToken);
        http.DefaultRequestHeaders.UserAgent.ParseAdd("JobTrackrApi");

        var profile = JsonSerializer.Deserialize<GitHubProfile>(
            await http.GetStringAsync("https://api.github.com/user"),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Provider == "GitHub" && u.ProviderId == profile.Id.ToString());
        if (user == null)
        {
            user = new AppUser
            {
                Email = profile.Email ?? $"{profile.Login}@users.noreply.github.com",
                Name = profile.Name ?? profile.Login,
                Provider = "GitHub",
                ProviderId = profile.Id.ToString()
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var (accessToken, refreshToken) = await IssueTokensAsync(user);
        return Ok(new { accessToken, refreshToken });
    }

    public record RefreshRequest(string RefreshToken);

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (storedToken == null || !storedToken.IsActive)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }

        storedToken.RevokedAt = DateTime.UtcNow;

        var user = await _context.Users.FindAsync(storedToken.UserId);
        if (user == null) return Unauthorized();

        var (accessToken, newRefreshToken) = await IssueTokensAsync(user);
        await _context.SaveChangesAsync();

        return Ok(new { accessToken, refreshToken = newRefreshToken });
    }

    public record LogoutRequest(string RefreshToken);

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (storedToken != null)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }
}