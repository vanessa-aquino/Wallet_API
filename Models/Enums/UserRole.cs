using System.ComponentModel.DataAnnotations;

namespace WalletAPI.Models.Enums
{
    public enum UserRole
    {

        [Display(Name = "Usuario")] User,
        [Display(Name = "Admin")] Admin
    }
}
