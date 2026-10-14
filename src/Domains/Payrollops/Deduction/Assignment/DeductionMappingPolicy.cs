namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of deduction mapping policy.</summary>
public static class DeductionMappingPolicy
{
    public static double DeductionMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int DeductionMappingSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a deduction mapping score falls in.</summary>
    public static string ClassifyDeductionMapping(double score)
    {
        if (score < 33.9)
        {
            return "elevated";
        }
        if (score < 51.5)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 40144.0, in order.</summary>
    public static int DeductionMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 40144.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
