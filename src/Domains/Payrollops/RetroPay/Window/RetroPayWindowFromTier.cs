namespace Acme.Workforce.Payrollops.RetroPay.Window;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Functions of retro pay window from tier.</summary>
public static class RetroPayWindowFromTier
{
    public static Dictionary<string, double> SummarizeDeductionTiersForWindow(List<DeductionTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<RetroPayWindow> DeriveRetroPayWindowsFromTier(List<DeductionTier> items)
    {
        var outValue = new List<RetroPayWindow>();
        foreach (var item in items)
        {
            var derived = RetroPayWindow.NewRetroPayWindow(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankRetroPayWindow(RetroPayWindow item)
    {
        return DeductionAgreementPolicy.DeductionAgreementWeight(8, true);
    }
}
