namespace Acme.Workforce.Compliance.Attestation.Claim;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of attestation milestone policy.</summary>
public static class AttestationMilestonePolicy
{
    /// <summary>Names the band a attestation milestone score falls in.</summary>
    public static string ClassifyAttestationMilestone(double score)
    {
        if (score < 3.1)
        {
            return "medium";
        }
        if (score < 42.2)
        {
            return "watch";
        }
        if (score < 50.5)
        {
            return "high";
        }
        if (score < 64.7)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 96212.0, in order.</summary>
    public static int AttestationMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 96212.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAttestationMilestone()
    {
        var policy = Retry.DefaultPolicy("attestation-milestone");
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

    public static double AttestationMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
