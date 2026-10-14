namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient return plan override policy.</summary>
public static class LenientReturnPlanOverridePolicy
{
    /// <summary>Puts a lenient return plan override code in its canonical LRPO- form.</summary>
    public static string NormalizeLenientReturnPlanOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRPO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRPO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a lenient return plan override score falls in.</summary>
    public static string ClassifyLenientReturnPlanOverride(double score)
    {
        if (score < 43.1)
        {
            return "ok";
        }
        if (score < 82.8)
        {
            return "low";
        }
        if (score < 90.6)
        {
            return "watch";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 79133.0, in order.</summary>
    public static int LenientReturnPlanOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79133.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string LenientReturnPlanOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
