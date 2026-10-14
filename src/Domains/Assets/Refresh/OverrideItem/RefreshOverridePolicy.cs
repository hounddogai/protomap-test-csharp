namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of refresh override policy.</summary>
public static class RefreshOverridePolicy
{
    /// <summary>Names the band a refresh override score falls in.</summary>
    public static string ClassifyRefreshOverride(double score)
    {
        if (score < 57.2)
        {
            return "high";
        }
        if (score < 86.3)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRefreshOverride()
    {
        var policy = Retry.DefaultPolicy("refresh-override");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
