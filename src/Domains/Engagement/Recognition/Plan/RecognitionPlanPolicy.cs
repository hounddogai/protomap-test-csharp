namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition plan policy.</summary>
public static class RecognitionPlanPolicy
{
    public static double RecognitionPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.74;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a recognition plan code in its canonical RP- form.</summary>
    public static string NormalizeRecognitionPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 55862.0, in order.</summary>
    public static int RecognitionPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 55862.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a recognition plan score falls in.</summary>
    public static string ClassifyRecognitionPlan(double score)
    {
        if (score < 32.5)
        {
            return "low";
        }
        if (score < 47.6)
        {
            return "elevated";
        }
        if (score < 81.1)
        {
            return "minimal";
        }
        return "ok";
    }
}
