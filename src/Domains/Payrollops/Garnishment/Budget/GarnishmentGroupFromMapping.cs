namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Functions of garnishment group from mapping.</summary>
public static class GarnishmentGroupFromMapping
{
    public static Dictionary<string, double> SummarizePayRunMappingsForGroup(List<PayRunMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<GarnishmentGroup> DeriveGarnishmentGroupsFromMapping(List<PayRunMapping> items)
    {
        var outValue = new List<GarnishmentGroup>();
        foreach (var item in items)
        {
            var derived = GarnishmentGroup.NewGarnishmentGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeGarnishmentGroup(GarnishmentGroup item)
    {
        return PayRunLedgerPolicy.PayRunLedgerBucket(item.WeightedLoad());
    }
}
