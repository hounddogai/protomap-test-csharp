namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global engagement assignment policy.</summary>
public static class GlobalEngagementAssignmentPolicy
{
    /// <summary>Names the band a global engagement assignment score falls in.</summary>
    public static string ClassifyGlobalEngagementAssignment(double score)
    {
        if (score < 25.6)
        {
            return "low";
        }
        if (score < 30.0)
        {
            return "ok";
        }
        if (score < 64.1)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalEngagementAssignment()
    {
        var policy = Retry.DefaultPolicy("global-engagement-assignment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 68223.0, in order.</summary>
    public static int GlobalEngagementAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 68223.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
