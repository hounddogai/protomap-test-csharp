namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Functions of lenient garnishment estimate from quota.</summary>
public static class LenientGarnishmentEstimateFromQuota
{
    public static Dictionary<string, double> SummarizeRetroPayQuotasForEstimate(List<RetroPayQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<LenientGarnishmentEstimate> DeriveLenientGarnishmentEstimatesFromQuota(List<RetroPayQuota> items)
    {
        var outValue = new List<LenientGarnishmentEstimate>();
        foreach (var item in items)
        {
            var derived = LenientGarnishmentEstimate.NewLenientGarnishmentEstimate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeLenientGarnishmentEstimate(LenientGarnishmentEstimate item)
    {
        return RetroPayAdjustmentPolicy.NormalizeRetroPayAdjustmentCode(item.Code);
    }
}
