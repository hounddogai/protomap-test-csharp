namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional warranty proposal policy.</summary>
public static class RegionalWarrantyProposalPolicy
{
    /// <summary>Names the band a regional warranty proposal score falls in.</summary>
    public static string ClassifyRegionalWarrantyProposal(double score)
    {
        if (score < 68.1)
        {
            return "minimal";
        }
        if (score < 68.6)
        {
            return "ok";
        }
        if (score < 89.0)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalWarrantyProposal()
    {
        var policy = Retry.DefaultPolicy("regional-warranty-proposal");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
