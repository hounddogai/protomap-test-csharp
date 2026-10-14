namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary attestation policy policy.</summary>
public static class PrimaryAttestationPolicyPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryAttestationPolicy()
    {
        var policy = Retry.DefaultPolicy("primary-attestation-policy");
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

    public static double PrimaryAttestationPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.57;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 61189.0, in order.</summary>
    public static int PrimaryAttestationPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61189.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a primary attestation policy score falls in.</summary>
    public static string ClassifyPrimaryAttestationPolicy(double score)
    {
        if (score < 4.5)
        {
            return "watch";
        }
        if (score < 21.3)
        {
            return "critical";
        }
        if (score < 47.8)
        {
            return "medium";
        }
        if (score < 55.5)
        {
            return "low";
        }
        return "high";
    }
}
