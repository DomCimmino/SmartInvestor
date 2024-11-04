using Newtonsoft.Json;

namespace SmartInvestor.Models;

public class Facts
{
    [JsonProperty("dei")] public DocumentAndEntityInformation? DocumentAndEntityInformation { get; set; }
    [JsonProperty("us-gaap")] public FinancialReportingTaxonomy? FinancialReportingTaxonomy { get; set; }
}

public class DocumentAndEntityInformation
{
    [JsonProperty(nameof(EntityPublicFloat))] public BasicFact? EntityPublicFloat { get; set; }
}

public class FinancialReportingTaxonomy
{
    [JsonProperty(nameof(Liabilities))] public BasicFact? Liabilities { get; set; }
    [JsonProperty(nameof(Assets))] public BasicFact? Assets { get; set; }
}

public class BasicFact
{
    [JsonProperty("label")] public string? Label { get; set; }
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("units")] public Unit? Unit { get; set; }
}