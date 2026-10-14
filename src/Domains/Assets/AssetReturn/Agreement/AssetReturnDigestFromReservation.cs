namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Assets.LicenseSeat.SeatReservation;

/// <summary>Functions of asset return digest from reservation.</summary>
public static class AssetReturnDigestFromReservation
{
    public static Dictionary<string, double> SummarizeFallbackLicenseSeatReservationsForDigest(List<FallbackLicenseSeatReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<AssetReturnDigest> DeriveAssetReturnDigestsFromReservation(List<FallbackLicenseSeatReservation> items)
    {
        var outValue = new List<AssetReturnDigest>();
        foreach (var item in items)
        {
            var derived = AssetReturnDigest.NewAssetReturnDigest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateAssetReturnDigest(AssetReturnDigest item)
    {
        return LicenseSeatWindowPolicy.NormalizeLicenseSeatWindowCode(item.Code);
    }
}
