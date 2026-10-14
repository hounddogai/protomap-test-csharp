namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Deduction.Review;

/// <summary>Functions of regional payslip exception from notice.</summary>
public static class RegionalPayslipExceptionFromNotice
{
    public static Dictionary<string, double> SummarizeManualDeductionNoticesForException(List<ManualDeductionNotice> items)
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

    public static List<RegionalPayslipException> DeriveRegionalPayslipExceptionsFromNotice(List<ManualDeductionNotice> items)
    {
        var outValue = new List<RegionalPayslipException>();
        foreach (var item in items)
        {
            var derived = RegionalPayslipException.NewRegionalPayslipException(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeRegionalPayslipException(RegionalPayslipException item)
    {
        return ManualDeductionNoticePolicy.ManualDeductionNoticeBucket(item.EffectiveExposure());
    }
}
