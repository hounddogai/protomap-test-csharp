namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Functions of global rate exception from cycle.</summary>
public static class GlobalRateExceptionFromCycle
{
    public static Dictionary<string, double> SummarizeDefaultRateCyclesForException(List<DefaultRateCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<GlobalRateException> DeriveGlobalRateExceptionsFromCycle(List<DefaultRateCycle> items)
    {
        var outValue = new List<GlobalRateException>();
        foreach (var item in items)
        {
            var derived = GlobalRateException.NewGlobalRateException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeGlobalRateExceptionExtra(GlobalRateException item)
    {
        return AdaptiveRateTemplatePolicy.ClassifyAdaptiveRateTemplate(item.MeasuredValue());
    }
}
