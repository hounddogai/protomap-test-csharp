namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Functions of strict vendor request from snapshot.</summary>
public static class StrictVendorRequestFromSnapshot
{
    public static Dictionary<string, double> SummarizeStatementOfWorkSnapshotsForRequest(List<StatementOfWorkSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<StrictVendorRequest> DeriveStrictVendorRequestsFromSnapshot(List<StatementOfWorkSnapshot> items)
    {
        var outValue = new List<StrictVendorRequest>();
        foreach (var item in items)
        {
            var derived = StrictVendorRequest.NewStrictVendorRequest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeStrictVendorRequest(StrictVendorRequest item)
    {
        return GlobalStatementOfWorkTargetPolicy.ClassifyGlobalStatementOfWorkTarget(item.WeightedValue());
    }
}
