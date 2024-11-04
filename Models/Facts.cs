using Newtonsoft.Json;

namespace SmartInvestor.Models;

public class Facts
{
    [JsonProperty("dei")] public DocumentAndEntityInformation? DocumentAndEntityInformation { get; set; } 
    // TO-DO 
    // Property in FinancialReportingTaxonomy
   // [JsonProperty("us-gaap")] public FinancialReportingTaxonomy? FinancialReportingTaxonomy { get; set; }
}

public class DocumentAndEntityInformation
{
    [JsonProperty("EntityPublicFloat")] public BasicFact? EntityPublicFloat { get; set; }
}

public class FinancialReportingTaxonomy
{
    
}

public class BasicFact
{
    [JsonProperty("label")] public string? Label { get; set; }
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("units")] public Unit? Unit { get; set; }
}