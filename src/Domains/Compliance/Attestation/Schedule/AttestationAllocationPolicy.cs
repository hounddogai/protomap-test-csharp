namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of attestation allocation policy.</summary>
public static class AttestationAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 57241.0, in order.</summary>
    public static int AttestationAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57241.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a attestation allocation score falls in.</summary>
    public static string ClassifyAttestationAllocation(double score)
    {
        if (score < 6.5)
        {
            return "elevated";
        }
        if (score < 28.9)
        {
            return "low";
        }
        if (score < 43.0)
        {
            return "watch";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAttestationAllocation()
    {
        var policy = Retry.DefaultPolicy("attestation-allocation");
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

    public static double AttestationAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
