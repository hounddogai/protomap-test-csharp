namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of mobility proposal policy.</summary>
public static class MobilityProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeMobilityProposal()
    {
        var policy = Retry.DefaultPolicy("mobility-proposal");
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

    /// <summary>Puts a mobility proposal code in its canonical MP- form.</summary>
    public static string NormalizeMobilityProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a mobility proposal score falls in.</summary>
    public static string ClassifyMobilityProposal(double score)
    {
        if (score < 77.0)
        {
            return "minimal";
        }
        if (score < 89.0)
        {
            return "elevated";
        }
        if (score < 98.2)
        {
            return "low";
        }
        return "watch";
    }
}
