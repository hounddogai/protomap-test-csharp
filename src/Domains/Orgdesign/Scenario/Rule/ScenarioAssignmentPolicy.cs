namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scenario assignment policy.</summary>
public static class ScenarioAssignmentPolicy
{
    /// <summary>Names the band a scenario assignment score falls in.</summary>
    public static string ClassifyScenarioAssignment(double score)
    {
        if (score < 41.9)
        {
            return "low";
        }
        if (score < 50.5)
        {
            return "ok";
        }
        if (score < 99.9)
        {
            return "minimal";
        }
        return "critical";
    }

    public static string ScenarioAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a scenario assignment code in its canonical SA- form.</summary>
    public static string NormalizeScenarioAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
