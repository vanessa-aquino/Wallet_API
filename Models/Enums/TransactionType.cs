using System.ComponentModel.DataAnnotations;

namespace WalletAPI.Models.Enums
{
    public enum TransactionType
    {

        [Display(Name = "Deposito")] Deposit,
        [Display(Name = "Saque")] Withdraw,
        [Display(Name = "Transaferencia")] Transfer,
        [Display(Name = "Reembolso")] Refund
    }
}
