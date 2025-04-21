using System.ComponentModel.DataAnnotations;

namespace NetManagement.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ComputerId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        [CustomValidation(typeof(Order), nameof(ValidateEndTime))]
        public DateTime? EndTime { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Total cost must be a non-negative value.")]
        public decimal TotalCost { get; set; }


        public DateTime CreateAt { get; set; } = DateTime.Now;

        public bool isPay { get; set; } = false;

        public User User { get; set; }
        public Computer Computer { get; set; }

        public static ValidationResult? ValidateEndTime(DateTime? endTime, ValidationContext context)
        {
            var instance = context.ObjectInstance as Order;
            if (instance != null && endTime.HasValue && endTime <= instance.StartTime)
            {
                return new ValidationResult("End time must be greater than start time.");
            }
            return ValidationResult.Success;
        }
    }
}
