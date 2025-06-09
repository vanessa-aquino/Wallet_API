namespace WalletAPI.Models.DTOs
{
    public class PagedResultDto<T>
    {
        public List<T> Users { get; set; }
        public int TotalUsers { get; set; }
        public int Page {  get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
