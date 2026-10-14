namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of training due summary policy.</summary>
public static class TrainingDueSummaryPolicy
{
    /// <summary>Names the band a training due summary score falls in.</summary>
    public static string ClassifyTrainingDueSummary(double score)
    {
        if (score < 28.8)
        {
            return "low";
        }
        if (score < 45.2)
        {
            return "critical";
        }
        if (score < 66.8)
        {
            return "ok";
        }
        if (score < 75.9)
        {
            return "watch";
        }
        return "high";
    }

    public static double TrainingDueSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a training due summary code in its canonical TDS- form.</summary>
    public static string NormalizeTrainingDueSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TDS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TDS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TrainingDueSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }
}
