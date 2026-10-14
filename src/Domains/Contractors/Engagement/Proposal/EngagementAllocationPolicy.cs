namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of engagement allocation policy.</summary>
public static class EngagementAllocationPolicy
{
    /// <summary>Names the band a engagement allocation score falls in.</summary>
    public static string ClassifyEngagementAllocation(double score)
    {
        if (score < 60.1)
        {
            return "medium";
        }
        if (score < 81.8)
        {
            return "elevated";
        }
        if (score < 87.0)
        {
            return "high";
        }
        if (score < 98.5)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 6875.0, in order.</summary>
    public static int EngagementAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6875.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEngagementAllocation()
    {
        var policy = Retry.DefaultPolicy("engagement-allocation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string EngagementAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
