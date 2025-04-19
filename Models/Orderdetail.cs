using System.ComponentModel.DataAnnotations;

namespace NetManagement.Models
{
    public class Orderdetail
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int FoodId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be a non-negative value.")]
        public decimal UnitPrice { get; set; }

        public Order Order { get; set; }
    }
}
