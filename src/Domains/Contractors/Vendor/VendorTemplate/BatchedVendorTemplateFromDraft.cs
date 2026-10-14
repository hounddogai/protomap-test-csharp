namespace Acme.Workforce.Contractors.Vendor.VendorTemplate;

using Acme.Workforce.Contractors.Offboarding.Draft;

/// <summary>Functions of batched vendor template from draft.</summary>
public static class BatchedVendorTemplateFromDraft
{
    public static Dictionary<string, double> SummarizeOffboardingDraftsForTemplate(List<OffboardingDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<BatchedVendorTemplate> DeriveBatchedVendorTemplatesFromDraft(List<OffboardingDraft> items)
    {
        var outValue = new List<BatchedVendorTemplate>();
        foreach (var item in items)
        {
            var derived = BatchedVendorTemplate.NewBatchedVendorTemplate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchBatchedVendorTemplate(BatchedVendorTemplate item)
    {
        return OffboardingDraftPolicy.NormalizeOffboardingDraftCode(item.Code);
    }
}
