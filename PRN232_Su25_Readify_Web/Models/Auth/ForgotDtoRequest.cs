﻿using System.ComponentModel.DataAnnotations;

namespace PRN232_Su25_Readify_Web.Models.Auth
{
    public class ForgotDtoRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email format.")]
        public string Email { get; set; }
    }
}
