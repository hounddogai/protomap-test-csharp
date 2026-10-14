namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scenario schedule policy.</summary>
public static class ScenarioSchedulePolicy
{
    public static string ScenarioScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double ScenarioScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a scenario schedule code in its canonical SS- form.</summary>
    public static string NormalizeScenarioScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
