namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Functions of refresh request from agreement.</summary>
public static class RefreshRequestFromAgreement
{
    public static Dictionary<string, double> SummarizeLaptopAgreementsForRequest(List<LaptopAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<RefreshRequest> DeriveRefreshRequestsFromAgreement(List<LaptopAgreement> items)
    {
        var outValue = new List<RefreshRequest>();
        foreach (var item in items)
        {
            var derived = RefreshRequest.NewRefreshRequest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeRefreshRequest(RefreshRequest item)
    {
        return TieredLaptopCheckpointPolicy.TieredLaptopCheckpointBucket(item.AdjustedVolume());
    }
}
