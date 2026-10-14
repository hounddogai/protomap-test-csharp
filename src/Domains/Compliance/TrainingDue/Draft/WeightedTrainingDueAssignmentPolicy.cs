namespace Acme.Workforce.Compliance.TrainingDue.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted training due assignment policy.</summary>
public static class WeightedTrainingDueAssignmentPolicy
{
    public static List<string> MergeWeightedTrainingDueAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a weighted training due assignment score falls in.</summary>
    public static string ClassifyWeightedTrainingDueAssignment(double score)
    {
        if (score < 60.8)
        {
            return "elevated";
        }
        if (score < 61.1)
        {
            return "medium";
        }
        if (score < 84.4)
        {
            return "low";
        }
        return "minimal";
    }

    public static string WeightedTrainingDueAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double WeightedTrainingDueAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
