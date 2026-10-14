namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Functions of payslip digest from assignment.</summary>
public static class PayslipDigestFromAssignment
{
    public static Dictionary<string, double> SummarizePayRunAssignmentsForDigest(List<PayRunAssignment> items)
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

    public static List<PayslipDigest> DerivePayslipDigestsFromAssignment(List<PayRunAssignment> items)
    {
        var outValue = new List<PayslipDigest>();
        foreach (var item in items)
        {
            var derived = PayslipDigest.NewPayslipDigest(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocatePayslipDigest(PayslipDigest item)
    {
        return PayRunTargetPolicy.PayRunTargetWeight(5, true);
    }
}
