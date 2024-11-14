using Newtonsoft.Json;

namespace SmartInvestor.Models;

public class Facts
{
    [JsonProperty("dei")] public DocumentAndEntityInformation? DocumentAndEntityInformation { get; set; }
    [JsonProperty("us-gaap")] public FinancialReportingTaxonomy? FinancialReportingTaxonomy { get; set; }

    public bool HasAllNonNullProperties()
    {
        return DocumentAndEntityInformation?.HasAllNonNullProperties() == true &&
               FinancialReportingTaxonomy?.HasAllNonNullProperties() == true;
    }
}

public class DocumentAndEntityInformation
{
    [JsonProperty(nameof(EntityPublicFloat))]
    public BasicFact? EntityPublicFloat { get; set; }

    public bool HasAllNonNullProperties()
    {
        return EntityPublicFloat?.HasAllNonNullProperties() == true;
    }
}

public class FinancialReportingTaxonomy
{
    [JsonProperty(nameof(Liabilities))] public BasicFact? Liabilities { get; set; }
    [JsonProperty(nameof(Assets))] public BasicFact? Assets { get; set; }
    [JsonProperty("AssetsCurrent")] public BasicFact? CurrentAssets { get; set; }

    [JsonProperty("EarningsPerShareBasic")]
    public BasicFact? EarningsPerShare { get; set; }

    [JsonProperty(nameof(CommonStockSharesOutstanding))]
    public BasicFact? CommonStockSharesOutstanding { get; set; }

    [JsonProperty("PaymentsOfDividends")]
    public BasicFact? PaidDividends { get; set; }
    
    public bool HasAllNonNullProperties()
    {
        return Liabilities?.HasAllNonNullProperties() == true &&
               Assets?.HasAllNonNullProperties() == true &&
               CurrentAssets?.HasAllNonNullProperties() == true &&
               EarningsPerShare?.HasAllNonNullProperties() == true &&
               PaidDividends?.HasAllNonNullProperties() == true &&
               CommonStockSharesOutstanding?.HasAllNonNullProperties() == true;
    }
}

public class BasicFact
{
    [JsonProperty("label")] public string? Label { get; set; }
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("units")] public Unit? Unit { get; set; }

    public bool HasAllNonNullProperties()
    {
        return Label != null && Description != null && Unit?.HasAllNonNullProperties() == true;
    }
}