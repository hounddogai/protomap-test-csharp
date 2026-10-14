namespace Acme.Workforce.Compliance.Audit.AuditMapping;

using Acme.Workforce.Compliance.Audit.AuditDraft;

/// <summary>Functions of default audit mapping from draft.</summary>
public static class DefaultAuditMappingFromDraft
{
    public static Dictionary<string, double> SummarizeRegionalAuditDraftsForMapping(List<RegionalAuditDraft> items)
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

    public static List<DefaultAuditMapping> DeriveDefaultAuditMappingsFromDraft(List<RegionalAuditDraft> items)
    {
        var outValue = new List<DefaultAuditMapping>();
        foreach (var item in items)
        {
            var derived = DefaultAuditMapping.NewDefaultAuditMapping(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyDefaultAuditMapping(DefaultAuditMapping item)
    {
        return AuditSegmentPolicy.AuditSegmentBucket(item.AdjustedLoad());
    }
}
