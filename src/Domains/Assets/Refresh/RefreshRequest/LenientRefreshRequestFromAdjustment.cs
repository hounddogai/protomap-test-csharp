namespace Acme.Workforce.Assets.Refresh.RefreshRequest;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Functions of lenient refresh request from adjustment.</summary>
public static class LenientRefreshRequestFromAdjustment
{
    public static Dictionary<string, double> SummarizeDefaultLaptopAdjustmentsForRequest(List<DefaultLaptopAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<LenientRefreshRequest> DeriveLenientRefreshRequestsFromAdjustment(List<DefaultLaptopAdjustment> items)
    {
        var outValue = new List<LenientRefreshRequest>();
        foreach (var item in items)
        {
            var derived = LenientRefreshRequest.NewLenientRefreshRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLenientRefreshRequest(LenientRefreshRequest item)
    {
        return CappedLaptopRecordPolicy.ClassifyCappedLaptopRecord(item.ProjectedTotal());
    }
}
