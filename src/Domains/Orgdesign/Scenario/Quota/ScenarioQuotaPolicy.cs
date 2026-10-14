namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scenario quota policy.</summary>
public static class ScenarioQuotaPolicy
{
    public static int ScenarioQuotaSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a scenario quota code in its canonical SQ- form.</summary>
    public static string NormalizeScenarioQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScenarioQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
