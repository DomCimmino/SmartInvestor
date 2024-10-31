namespace SmartInvestor.Models;

public class Company
{
    public string? Symbol { get; set; }
    public string? Name { get; set; }
    public string? GicsSector { get; set; }
    public string? GicsSubIndustry { get; set; }
    public string? HeadquartersLocation { get; set; }
    public DateTime? AddedDateInSp500 { get; set; }
    public string? Cik { get; set; }
    public string? FoundedYear { get; set; }
}