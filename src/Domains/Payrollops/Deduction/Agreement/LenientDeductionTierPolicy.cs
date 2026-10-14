namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient deduction tier policy.</summary>
public static class LenientDeductionTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 72675.0, in order.</summary>
    public static int LenientDeductionTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72675.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a lenient deduction tier score falls in.</summary>
    public static string ClassifyLenientDeductionTier(double score)
    {
        if (score < 20.7)
        {
            return "elevated";
        }
        if (score < 47.8)
        {
            return "high";
        }
        if (score < 62.6)
        {
            return "medium";
        }
        if (score < 87.2)
        {
            return "minimal";
        }
        return "low";
    }

    public static double LenientDeductionTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LenientDeductionTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
