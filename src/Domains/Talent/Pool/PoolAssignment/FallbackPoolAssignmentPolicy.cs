namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback pool assignment policy.</summary>
public static class FallbackPoolAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 63145.0, in order.</summary>
    public static int FallbackPoolAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63145.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int FallbackPoolAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static List<string> MergeFallbackPoolAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
