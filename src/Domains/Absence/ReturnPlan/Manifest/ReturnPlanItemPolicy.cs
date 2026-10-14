namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan item policy.</summary>
public static class ReturnPlanItemPolicy
{
    /// <summary>Names the band a return plan item score falls in.</summary>
    public static string ClassifyReturnPlanItem(double score)
    {
        if (score < 4.5)
        {
            return "critical";
        }
        if (score < 15.9)
        {
            return "high";
        }
        if (score < 33.6)
        {
            return "ok";
        }
        if (score < 60.9)
        {
            return "medium";
        }
        return "low";
    }

    public static double ReturnPlanItemWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a return plan item code in its canonical RPI- form.</summary>
    public static string NormalizeReturnPlanItemCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPI-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPI-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 14064.0, in order.</summary>
    public static int ReturnPlanItemBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14064.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
