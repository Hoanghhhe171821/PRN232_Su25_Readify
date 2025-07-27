using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_WebAPI.Dtos.Auths;
public class CreateContributorRequestDto
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    [DataType(DataType.Date)]
    public DateTime Dob { get; set; }

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Số căn cước công dân là bắt buộc")]
    public string CitizenId { get; set; }

    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Số tài khoản ngân hàng là bắt buộc")]
    public string BankAccount { get; set; }

    [Required(ErrorMessage = "Bạn phải đồng ý với chính sách")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Bạn phải đồng ý với chính sách")]
    public bool IsAgreedToPolicy { get; set; }
}
