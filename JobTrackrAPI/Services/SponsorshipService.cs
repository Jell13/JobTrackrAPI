using JobTrackrAPI.Data;
using JobTrackrAPI.Models;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;

namespace JobTrackrAPI.Services
{
    public class SponsorshipService
    {
        private readonly AppDbContext _context;
        private readonly ChatClient _chatClient;
        public SponsorshipService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            var apiKey = configuration["OpenAI:ApiKey"];
            _chatClient = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);
        }

        public async Task<EmployerSponsorshipStat?> GetEmployerStatsAsync(string employerName)
        {
            var normalizedInput = employerName.Trim();

            return await _context.EmployerSponsorshipStats
                .FromSqlInterpolated($@"
                    SELECT employer_key, employer_name, total_filings, certified_filings,
                           certification_rate, typical_wage_from, typical_wage_to,
                           top_job_title, most_recent_decision, updated_at
                    FROM employer_sponsorship_stats
                    WHERE employer_name % {normalizedInput}
                    ORDER BY similarity(employer_name, {normalizedInput}) DESC
                    LIMIT 1")
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetSponsorshipInsightAsync(string employerName)
        {
            var stats = await GetEmployerStatsAsync(employerName);

            if (stats is null)
            {
                return $"No H-1B sponsorship filings found for '{employerName}' in this dataset.";
            }

            var prompt = $"""
                A user is evaluating a job posting from this employer. Here is their real
                H-1B sponsorship filing history from Department of Labor public data:

                Employer: {stats.EmployerName}
                Total LCA filings: {stats.TotalFilings}
                Certification rate: {stats.CertificationRate:P0}
                Typical wage range: ${stats.TypicalWageFrom:N0} - ${stats.TypicalWageTo:N0}
                Most common sponsored title: {stats.TopJobTitle}
                Most recent filing decision: {stats.MostRecentDecision:d}

                Explain in 2-3 plain-English sentences what this means for an international
                student considering applying here. Be specific about what the numbers
                actually indicate, and don't overstate certainty - a high certification
                rate means DOL approved the paperwork, not that USCIS approved an actual visa.
                """;

            ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);
            return completion.Content[0].Text;
        }
    }
}
