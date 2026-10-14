namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment checkpoint policy.</summary>
public static class AssignmentCheckpointPolicy
{
    public static List<string> MergeAssignmentCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a assignment checkpoint score falls in.</summary>
    public static string ClassifyAssignmentCheckpoint(double score)
    {
        if (score < 8.5)
        {
            return "minimal";
        }
        if (score < 32.0)
        {
            return "ok";
        }
        if (score < 59.6)
        {
            return "critical";
        }
        if (score < 61.7)
        {
            return "high";
        }
        return "elevated";
    }

    public static string AssignmentCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
