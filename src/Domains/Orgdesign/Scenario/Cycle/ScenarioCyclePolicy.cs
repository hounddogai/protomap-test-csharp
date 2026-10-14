namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Functions of scenario cycle policy.</summary>
public static class ScenarioCyclePolicy
{
    /// <summary>Names the band a scenario cycle score falls in.</summary>
    public static string ClassifyScenarioCycle(double score)
    {
        if (score < 18.9)
        {
            return "watch";
        }
        if (score < 72.7)
        {
            return "high";
        }
        if (score < 84.1)
        {
            return "minimal";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 90236.0, in order.</summary>
    public static int ScenarioCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90236.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a scenario cycle code in its canonical SC- form.</summary>
    public static string NormalizeScenarioCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ScenarioCycleSpan(int start, int end)
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
