namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of desk budget policy.</summary>
public static class DeskBudgetPolicy
{
    public static int DeskBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeskBudget()
    {
        var policy = Retry.DefaultPolicy("desk-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a desk budget score falls in.</summary>
    public static string ClassifyDeskBudget(double score)
    {
        if (score < 60.9)
        {
            return "watch";
        }
        if (score < 93.8)
        {
            return "critical";
        }
        return "elevated";
    }

    public static List<string> MergeDeskBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
