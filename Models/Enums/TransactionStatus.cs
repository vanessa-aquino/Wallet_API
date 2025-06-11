using System.ComponentModel.DataAnnotations;

namespace WalletAPI.Models.Enums
{
    public enum TransactionStatus
    {
        [Display(Name = "Pendente")] Pending,
        [Display(Name = "Concluido")] Completed,
        [Display(Name = "Falha")] Failed,
        [Display(Name = "Cancelada")] Canceled,
        [Display(Name = "Estornada")] Reversed,
        [Display(Name = "Processando")] Processing
    }
}
