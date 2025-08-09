using API.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace API.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Ip { get; set; } = "";
        public ApplicationUserStatus Status { get; set; } = ApplicationUserStatus.Attivo;

        public Cliente? Cliente { get; set; }
        public Carrello Carrello { get; set; } 


        public void ChangeStatus(ApplicationUserStatus newStatus)
        {
            Status = newStatus;
        }
        
    }
}
