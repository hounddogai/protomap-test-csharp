namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Functions of manual deduction notice from digest.</summary>
public static class ManualDeductionNoticeFromDigest
{
    public static Dictionary<string, double> SummarizeRollingPayRunDigestsForNotice(List<RollingPayRunDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<ManualDeductionNotice> DeriveManualDeductionNoticesFromDigest(List<RollingPayRunDigest> items)
    {
        var outValue = new List<ManualDeductionNotice>();
        foreach (var item in items)
        {
            var derived = ManualDeductionNotice.NewManualDeductionNotice(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveManualDeductionNotice(ManualDeductionNotice item)
    {
        return RollingPayRunDigestPolicy.NormalizeRollingPayRunDigestCode(item.Code);
    }
}
