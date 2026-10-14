namespace Acme.Workforce.Payrollops.TaxForm.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form milestone policy.</summary>
public static class TaxFormMilestonePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 91758.0, in order.</summary>
    public static int TaxFormMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91758.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a tax form milestone score falls in.</summary>
    public static string ClassifyTaxFormMilestone(double score)
    {
        if (score < 17.0)
        {
            return "elevated";
        }
        if (score < 40.2)
        {
            return "minimal";
        }
        if (score < 51.4)
        {
            return "medium";
        }
        if (score < 90.3)
        {
            return "high";
        }
        return "watch";
    }

    public static double TaxFormMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.38;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
