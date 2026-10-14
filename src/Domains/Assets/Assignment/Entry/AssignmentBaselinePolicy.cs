namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment baseline policy.</summary>
public static class AssignmentBaselinePolicy
{
    /// <summary>Names the band a assignment baseline score falls in.</summary>
    public static string ClassifyAssignmentBaseline(double score)
    {
        if (score < 16.1)
        {
            return "critical";
        }
        if (score < 19.0)
        {
            return "low";
        }
        if (score < 25.5)
        {
            return "watch";
        }
        if (score < 80.8)
        {
            return "high";
        }
        return "ok";
    }

    public static int AssignmentBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static List<string> MergeAssignmentBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 88667.0, in order.</summary>
    public static int AssignmentBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88667.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
