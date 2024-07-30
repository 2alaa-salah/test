using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TechnoEvents.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "You Should Enter a vaild Email"),
         EmailAddress(ErrorMessage = "You Should Enter a vaild Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "You Should Enter Your First Name"),
        Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "You Should Enter Your Last Name"),
        Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "You Should Enter Your Password"),
        DataType(DataType.Password),MinLength(3, ErrorMessage = "Password should be longer than 3chars")]
        public string Password { get; set; }
        [Required, Compare("Password", ErrorMessage = "your passworddoesn’t match!")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Image")]
        public string imgurl { get; set; }
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
