using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
namespace PhonePartsStore.Models
{
    [Table("Users")]
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên."), StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email."), EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu."), MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự."), MaxLength(100)]
        public string PasswordHash { get; set; }

        [NotMapped] 
        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [Compare("PasswordHash", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPasswordHash { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(20)]
        public string Role { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
