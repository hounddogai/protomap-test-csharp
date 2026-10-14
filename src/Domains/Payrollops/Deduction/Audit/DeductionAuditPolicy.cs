namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of deduction audit policy.</summary>
public static class DeductionAuditPolicy
{
    public static string DeductionAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a deduction audit score falls in.</summary>
    public static string ClassifyDeductionAudit(double score)
    {
        if (score < 22.7)
        {
            return "minimal";
        }
        if (score < 32.3)
        {
            return "elevated";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 97782.0, in order.</summary>
    public static int DeductionAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97782.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double DeductionAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
