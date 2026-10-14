namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Functions of weighted license seat estimate from agreement.</summary>
public static class WeightedLicenseSeatEstimateFromAgreement
{
    public static Dictionary<string, double> SummarizeAdaptiveLicenseSeatAgreementsForEstimate(List<AdaptiveLicenseSeatAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<WeightedLicenseSeatEstimate> DeriveWeightedLicenseSeatEstimatesFromAgreement(List<AdaptiveLicenseSeatAgreement> items)
    {
        var outValue = new List<WeightedLicenseSeatEstimate>();
        foreach (var item in items)
        {
            var derived = WeightedLicenseSeatEstimate.NewWeightedLicenseSeatEstimate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeWeightedLicenseSeatEstimate(WeightedLicenseSeatEstimate item)
    {
        return AdaptiveLicenseSeatAgreementPolicy.NormalizeAdaptiveLicenseSeatAgreementCode(item.Code);
    }
}
