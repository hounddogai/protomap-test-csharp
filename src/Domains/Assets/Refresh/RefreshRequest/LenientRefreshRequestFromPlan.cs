namespace Acme.Workforce.Assets.Refresh.RefreshRequest;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Functions of lenient refresh request from plan.</summary>
public static class LenientRefreshRequestFromPlan
{
    public static Dictionary<string, double> SummarizeRefreshPlansForRequest(List<RefreshPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<LenientRefreshRequest> DeriveLenientRefreshRequestsFromPlan(List<RefreshPlan> items)
    {
        var outValue = new List<LenientRefreshRequest>();
        foreach (var item in items)
        {
            var derived = LenientRefreshRequest.NewLenientRefreshRequest(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleLenientRefreshRequest(LenientRefreshRequest item)
    {
        return RefreshPlanPolicy.ClassifyRefreshPlan(item.ProjectedTotal());
    }
}
