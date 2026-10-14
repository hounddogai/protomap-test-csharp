namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict statement of work assignment policy.</summary>
public static class StrictStatementOfWorkAssignmentPolicy
{
    public static double StrictStatementOfWorkAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a strict statement of work assignment score falls in.</summary>
    public static string ClassifyStrictStatementOfWorkAssignment(double score)
    {
        if (score < 8.1)
        {
            return "critical";
        }
        if (score < 50.0)
        {
            return "high";
        }
        if (score < 63.0)
        {
            return "watch";
        }
        if (score < 68.8)
        {
            return "ok";
        }
        return "low";
    }

    public static int StrictStatementOfWorkAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static List<string> MergeStrictStatementOfWorkAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
