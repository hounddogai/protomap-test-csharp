namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of assignment entry policy.</summary>
public static class AssignmentEntryPolicy
{
    public static string AssignmentEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssignmentEntry()
    {
        var policy = Retry.DefaultPolicy("assignment-entry");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a assignment entry score falls in.</summary>
    public static string ClassifyAssignmentEntry(double score)
    {
        if (score < 8.1)
        {
            return "elevated";
        }
        if (score < 11.3)
        {
            return "high";
        }
        if (score < 76.1)
        {
            return "minimal";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 64026.0, in order.</summary>
    public static int AssignmentEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64026.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
