namespace JobTrackrAPI.Models
{
    public class Application
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Company { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        
        public DateTime AppliedDate { get; set; }
    }
}
