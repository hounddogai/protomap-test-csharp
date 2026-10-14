namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled training due draft policy.</summary>
public static class ScheduledTrainingDueDraftPolicy
{
    public static List<string> MergeScheduledTrainingDueDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int ScheduledTrainingDueDraftSpan(int start, int end)
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
        return span / 60;
    }

    public static string ScheduledTrainingDueDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a scheduled training due draft code in its canonical STDD- form.</summary>
    public static string NormalizeScheduledTrainingDueDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("STDD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"STDD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
