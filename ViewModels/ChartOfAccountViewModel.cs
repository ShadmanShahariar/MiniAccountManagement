namespace MiniAccountManagement.ViewModels
{
    public class ChartOfAccountViewModel
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public int? ParentId { get; set; }
        public string AccountType { get; set; }
        public bool IsDeleted { get; set; }

        public List<ChartOfAccountViewModel> Children { get; set; } = new();
    }

}
