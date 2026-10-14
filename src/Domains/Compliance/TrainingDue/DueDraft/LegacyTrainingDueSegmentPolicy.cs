namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy training due segment policy.</summary>
public static class LegacyTrainingDueSegmentPolicy
{
    public static string LegacyTrainingDueSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a legacy training due segment score falls in.</summary>
    public static string ClassifyLegacyTrainingDueSegment(double score)
    {
        if (score < 24.9)
        {
            return "medium";
        }
        if (score < 53.0)
        {
            return "minimal";
        }
        if (score < 63.7)
        {
            return "watch";
        }
        if (score < 77.5)
        {
            return "critical";
        }
        return "elevated";
    }

    public static int LegacyTrainingDueSegmentSpan(int start, int end)
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
}
