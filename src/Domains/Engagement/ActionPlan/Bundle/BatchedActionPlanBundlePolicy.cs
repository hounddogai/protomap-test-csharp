namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched action plan bundle policy.</summary>
public static class BatchedActionPlanBundlePolicy
{
    public static string BatchedActionPlanBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a batched action plan bundle code in its canonical BAPB- form.</summary>
    public static string NormalizeBatchedActionPlanBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BAPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BAPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeBatchedActionPlanBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double BatchedActionPlanBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
