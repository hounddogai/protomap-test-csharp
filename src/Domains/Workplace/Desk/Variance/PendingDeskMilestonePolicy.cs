namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending desk milestone policy.</summary>
public static class PendingDeskMilestonePolicy
{
    /// <summary>Names the band a pending desk milestone score falls in.</summary>
    public static string ClassifyPendingDeskMilestone(double score)
    {
        if (score < 21.2)
        {
            return "low";
        }
        if (score < 38.8)
        {
            return "medium";
        }
        if (score < 42.1)
        {
            return "ok";
        }
        return "elevated";
    }

    public static double PendingDeskMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a pending desk milestone code in its canonical PDM- form.</summary>
    public static string NormalizePendingDeskMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PDM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PDM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
