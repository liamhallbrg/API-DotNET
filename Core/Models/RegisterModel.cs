using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class RegisterModel
    {
        [Required]
        public string? Username { get; set; }

        [EmailAddress, Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
