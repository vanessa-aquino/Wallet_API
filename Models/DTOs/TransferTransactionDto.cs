﻿namespace WalletAPI.Models.DTOs
{
    public class TransferTransactionDto
    {
        public int SourceWalletId { get; set; }
        public int DestinationWalletId { get; set; }
        public double Amount { get; set; }
        public int UserId {  get; set; }
        public string? Description { get; set; }
    }
}
