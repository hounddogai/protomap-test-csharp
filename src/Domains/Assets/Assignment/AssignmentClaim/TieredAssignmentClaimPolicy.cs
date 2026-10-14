namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered assignment claim policy.</summary>
public static class TieredAssignmentClaimPolicy
{
    /// <summary>Names the band a tiered assignment claim score falls in.</summary>
    public static string ClassifyTieredAssignmentClaim(double score)
    {
        if (score < 43.0)
        {
            return "elevated";
        }
        if (score < 53.7)
        {
            return "high";
        }
        if (score < 87.3)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 8317.0, in order.</summary>
    public static int TieredAssignmentClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8317.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double TieredAssignmentClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.02;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredAssignmentClaim()
    {
        var policy = Retry.DefaultPolicy("tiered-assignment-claim");
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
}
