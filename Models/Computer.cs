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
    }

    public enum ComputerStatus
    {
        Available,
        InUse,
        Maintenance
    }
}
