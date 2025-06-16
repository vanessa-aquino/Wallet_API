namespace WalletAPI.Models.DTOs
{
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int Page {  get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
