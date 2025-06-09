namespace WalletAPI.Models.DTOs.User
{
    public class UserQueryParams
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search {  get; set; }
        public bool? Active { get; set; }
        public bool? OnlyDeleted { get; set; }
    }
}
