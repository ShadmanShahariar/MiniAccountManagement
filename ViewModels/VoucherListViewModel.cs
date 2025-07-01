namespace MiniAccountManagement.ViewModels
{
    public class VoucherListViewModel
    {
        public int Id { get; set; }
        public string VoucherType { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
