using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobTrackrAPI.Models
{
    [Table("employer_sponsorship_stats")]
    public class EmployerSponsorshipStat
    {
        [Key]
        [Column("employer_key")]
        public string EmployerKey { get; set; } = string.Empty;

        [Column("employer_name")]
        public string EmployerName { get; set; } = string.Empty;

        [Column("total_filings")]
        public int TotalFilings { get; set; }

        [Column("certified_filings")]
        public int CertifiedFilings { get; set; }

        [Column("certification_rate")]
        public decimal CertificationRate { get; set; }

        [Column("typical_wage_from")]
        public decimal? TypicalWageFrom { get; set; }

        [Column("typical_wage_to")]
        public decimal? TypicalWageTo { get; set; }

        [Column("top_job_title")]
        public string? TopJobTitle { get; set; }

        [Column("most_recent_decision")]
        public DateTime? MostRecentDecision { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}