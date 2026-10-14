namespace Acme.Workforce.Contractors.Rate.RateReview;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling rate assignment policy.</summary>
public static class RollingRateAssignmentPolicy
{
    /// <summary>Names the band a rolling rate assignment score falls in.</summary>
    public static string ClassifyRollingRateAssignment(double score)
    {
        if (score < 30.1)
        {
            return "elevated";
        }
        if (score < 91.1)
        {
            return "medium";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingRateAssignment()
    {
        var policy = Retry.DefaultPolicy("rolling-rate-assignment");
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

    /// <summary>Counts how many amounts fit in a budget of 19080.0, in order.</summary>
    public static int RollingRateAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19080.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
