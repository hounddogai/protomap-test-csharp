namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy desk manifest policy.</summary>
public static class LegacyDeskManifestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyDeskManifest()
    {
        var policy = Retry.DefaultPolicy("legacy-desk-manifest");
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

    /// <summary>Names the band a legacy desk manifest score falls in.</summary>
    public static string ClassifyLegacyDeskManifest(double score)
    {
        if (score < 3.1)
        {
            return "critical";
        }
        if (score < 3.6)
        {
            return "minimal";
        }
        if (score < 9.2)
        {
            return "elevated";
        }
        if (score < 33.7)
        {
            return "ok";
        }
        return "low";
    }
}
