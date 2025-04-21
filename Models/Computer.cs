using System.ComponentModel.DataAnnotations;

namespace NetManagement.Models
{
    public class Computer
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public ComputerStatus Status { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price per hour must be a non-negative value.")]
        public decimal PricePerHour { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Booking count must be a non-negative value.")]
        public int BookingCount { get; set; } = 0;
    }

    public enum ComputerStatus
    {
        Available,
        InUse,
        Maintenance
    }
}
