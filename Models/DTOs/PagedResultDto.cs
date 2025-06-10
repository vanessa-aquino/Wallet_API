namespace WalletAPI.Models.DTOs
{
    public class PagedResultDto<T>
    {
        public List<T> Users { get; set; } = new();
        public int TotalUsers { get; set; }

        public List<T> Data
        {
            get => Users;
            set => Users = value;
        }

        public int TotalItems
        {
            get => TotalUsers;
            set => TotalUsers = value;
        }

        public int Page {  get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
