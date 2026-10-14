namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>Functions of lenient retro pay exception from window.</summary>
public static class LenientRetroPayExceptionFromWindow
{
    public static Dictionary<string, double> SummarizeRetroPayWindowsForException(List<RetroPayWindow> items)
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

    public static List<LenientRetroPayException> DeriveLenientRetroPayExceptionsFromWindow(List<RetroPayWindow> items)
    {
        var outValue = new List<LenientRetroPayException>();
        foreach (var item in items)
        {
            var derived = LenientRetroPayException.NewLenientRetroPayException(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLenientRetroPayExceptionExtra(LenientRetroPayException item)
    {
        return RetroPayEstimatePolicy.ClassifyRetroPayEstimate(item.ProjectedTotal());
    }
}
