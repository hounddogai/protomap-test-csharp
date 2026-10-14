namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Functions of default payslip threshold from entry.</summary>
public static class DefaultPayslipThresholdFromEntry
{
    public static Dictionary<string, double> SummarizeDefaultPayslipEntriesForThreshold(List<DefaultPayslipEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<DefaultPayslipThreshold> DeriveDefaultPayslipThresholdsFromEntry(List<DefaultPayslipEntry> items)
    {
        var outValue = new List<DefaultPayslipThreshold>();
        foreach (var item in items)
        {
            var derived = DefaultPayslipThreshold.NewDefaultPayslipThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectDefaultPayslipThreshold(DefaultPayslipThreshold item)
    {
        return DefaultPayslipEntryPolicy.DefaultPayslipEntryBucket(item.MeasuredValue());
    }
}
