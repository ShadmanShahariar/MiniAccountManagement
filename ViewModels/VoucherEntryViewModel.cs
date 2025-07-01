public class VoucherEntryViewModel
{
    public DateTime VoucherDate { get; set; }
    public string ReferenceNo { get; set; }
    public string VoucherType { get; set; }

    public List<VoucherDetailViewModel> Entries { get; set; } = new();
}

public class VoucherDetailViewModel
{
    public int AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
