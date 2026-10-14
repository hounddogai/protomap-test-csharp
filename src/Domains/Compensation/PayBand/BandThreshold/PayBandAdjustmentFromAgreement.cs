namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Functions of pay band adjustment from agreement.</summary>
public static class PayBandAdjustmentFromAgreement
{
    public static Dictionary<string, double> SummarizeGlobalPayBandAgreementsForAdjustment(List<GlobalPayBandAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<PayBandAdjustment> DerivePayBandAdjustmentsFromAgreement(List<GlobalPayBandAgreement> items)
    {
        var outValue = new List<PayBandAdjustment>();
        foreach (var item in items)
        {
            var derived = PayBandAdjustment.NewPayBandAdjustment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePayBandAdjustment(PayBandAdjustment item)
    {
        return GlobalPayBandAgreementPolicy.GlobalPayBandAgreementBucket(item.ProjectedExposure());
    }
}
