namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of attestation segment policy.</summary>
public static class AttestationSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAttestationSegment()
    {
        var policy = Retry.DefaultPolicy("attestation-segment");
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

    public static double AttestationSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 13431.0, in order.</summary>
    public static int AttestationSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13431.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a attestation segment score falls in.</summary>
    public static string ClassifyAttestationSegment(double score)
    {
        if (score < 11.1)
        {
            return "minimal";
        }
        if (score < 56.3)
        {
            return "critical";
        }
        if (score < 74.3)
        {
            return "watch";
        }
        return "medium";
    }
}
