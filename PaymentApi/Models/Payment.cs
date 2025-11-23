using System.ComponentModel.DataAnnotations;

namespace PaymentApi.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string Reference { get; set; } = null!;
        [Range(0.01,double.MaxValue)]
        public decimal Amount { get; set; }
        [Required]
        public string Currency { get; set; } = null!;
        [Required]
        public string ClientRequestId { get; set; } = null!;
        public string? Provider { get; set; }
        public string? TransactionId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
