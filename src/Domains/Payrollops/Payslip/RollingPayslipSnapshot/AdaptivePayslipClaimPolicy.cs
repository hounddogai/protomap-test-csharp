namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive payslip claim policy.</summary>
public static class AdaptivePayslipClaimPolicy
{
    /// <summary>Names the band a adaptive payslip claim score falls in.</summary>
    public static string ClassifyAdaptivePayslipClaim(double score)
    {
        if (score < 53.3)
        {
            return "elevated";
        }
        if (score < 76.7)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 22702.0, in order.</summary>
    public static int AdaptivePayslipClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22702.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double AdaptivePayslipClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
