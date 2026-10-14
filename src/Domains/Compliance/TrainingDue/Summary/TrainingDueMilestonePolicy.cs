namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of training due milestone policy.</summary>
public static class TrainingDueMilestonePolicy
{
    public static string TrainingDueMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double TrainingDueMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int TrainingDueMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a training due milestone code in its canonical TDM- form.</summary>
    public static string NormalizeTrainingDueMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TDM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TDM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
