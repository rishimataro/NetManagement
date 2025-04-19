using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NetManagement.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }  // Khóa chính

        [Required(ErrorMessage = "Tên người dùng không được để trống")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; }

        [Required]
        public bool IsAdmin { get; set; }  // Phân quyền: true = admin, false = user

        [Range(0, double.MaxValue, ErrorMessage = "Số dư phải lớn hơn hoặc bằng 0")]
        public decimal Balance { get; set; }

        [MaxLength(200, ErrorMessage = "Đường dẫn ảnh không được vượt quá 200 ký tự")]
        public string ImageUrl { get; set; } = "~/images/avatar-cute-3.jpg";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
