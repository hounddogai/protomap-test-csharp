namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Functions of lenient visitor entry from agreement.</summary>
public static class LenientVisitorEntryFromAgreement
{
    public static Dictionary<string, double> SummarizeCappedFloorPlanAgreementsForEntry(List<CappedFloorPlanAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<LenientVisitorEntry> DeriveLenientVisitorEntriesFromAgreement(List<CappedFloorPlanAgreement> items)
    {
        var outValue = new List<LenientVisitorEntry>();
        foreach (var item in items)
        {
            var derived = LenientVisitorEntry.NewLenientVisitorEntry(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchLenientVisitorEntry(LenientVisitorEntry item)
    {
        return CappedFloorPlanAgreementPolicy.CappedFloorPlanAgreementBucket(item.NetTotal());
    }
}
