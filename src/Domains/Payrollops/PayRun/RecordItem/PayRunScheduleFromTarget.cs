namespace Acme.Workforce.Payrollops.PayRun.RecordItem;

using Acme.Workforce.Payrollops.TaxForm.Bundle;

/// <summary>Functions of pay run schedule from target.</summary>
public static class PayRunScheduleFromTarget
{
    public static Dictionary<string, double> SummarizeTaxFormTargetsForSchedule(List<TaxFormTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<PayRunSchedule> DerivePayRunSchedulesFromTarget(List<TaxFormTarget> items)
    {
        var outValue = new List<PayRunSchedule>();
        foreach (var item in items)
        {
            var derived = PayRunSchedule.NewPayRunSchedule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimatePayRunSchedule(PayRunSchedule item)
    {
        return TaxFormTargetPolicy.TaxFormTargetBucket(item.NetTotal());
    }
}
