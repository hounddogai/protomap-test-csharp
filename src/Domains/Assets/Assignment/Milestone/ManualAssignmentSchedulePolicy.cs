namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual assignment schedule policy.</summary>
public static class ManualAssignmentSchedulePolicy
{
    /// <summary>Names the band a manual assignment schedule score falls in.</summary>
    public static string ClassifyManualAssignmentSchedule(double score)
    {
        if (score < 65.8)
        {
            return "low";
        }
        if (score < 95.4)
        {
            return "ok";
        }
        return "minimal";
    }

    public static List<string> MergeManualAssignmentScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ManualAssignmentScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
