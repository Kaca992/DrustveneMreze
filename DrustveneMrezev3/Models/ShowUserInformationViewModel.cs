using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DrustveneMrezev3.Models
{
    public class ShowUserInformationViewModel
    {

        
        [Display(Name = "Email")]
        public string Email { get; set; }

        
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        
        [Display(Name = "Birthday")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Birthday { get; set; }

        public string FullName 
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
    }
}