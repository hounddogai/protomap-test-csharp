namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of training due segment policy.</summary>
public static class TrainingDueSegmentPolicy
{
    /// <summary>Puts a training due segment code in its canonical TDS- form.</summary>
    public static string NormalizeTrainingDueSegmentCode(string code)
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

    public static string TrainingDueSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int TrainingDueSegmentSpan(int start, int end)
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
        return span / 3600;
    }
}
