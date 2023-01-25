using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Infrastructure.DataTransferObjects
{
    public class ConsentFormDto
    {
        [Display(Name = "Application")]
        public string ApplicationName { get; set; }

        [Display(Name = "Scope")]
        public string Scope { get; set; }
    }
}
