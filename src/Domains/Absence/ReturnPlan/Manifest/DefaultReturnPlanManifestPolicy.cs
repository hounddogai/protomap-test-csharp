namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default return plan manifest policy.</summary>
public static class DefaultReturnPlanManifestPolicy
{
    public static string DefaultReturnPlanManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 93891.0, in order.</summary>
    public static int DefaultReturnPlanManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93891.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a default return plan manifest score falls in.</summary>
    public static string ClassifyDefaultReturnPlanManifest(double score)
    {
        if (score < 1.6)
        {
            return "elevated";
        }
        if (score < 10.5)
        {
            return "ok";
        }
        if (score < 38.7)
        {
            return "high";
        }
        if (score < 86.4)
        {
            return "critical";
        }
        return "minimal";
    }
}
