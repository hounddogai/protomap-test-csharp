namespace Acme.Workforce.Assets.AssetReturn.Request;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Functions of asset return request from cycle.</summary>
public static class AssetReturnRequestFromCycle
{
    public static Dictionary<string, double> SummarizePrimaryLicenseSeatCyclesForRequest(List<PrimaryLicenseSeatCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<AssetReturnRequest> DeriveAssetReturnRequestsFromCycle(List<PrimaryLicenseSeatCycle> items)
    {
        var outValue = new List<AssetReturnRequest>();
        foreach (var item in items)
        {
            var derived = AssetReturnRequest.NewAssetReturnRequest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleAssetReturnRequest(AssetReturnRequest item)
    {
        return AdaptiveLicenseSeatAgreementPolicy.AdaptiveLicenseSeatAgreementBucket(item.MeasuredValue());
    }
}
