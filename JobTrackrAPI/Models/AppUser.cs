namespace JobTrackrAPI.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Provider { get; set; } = string.Empty;
        public string ProviderId { get; set; } = string.Empty;
    }
}
