namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual scenario window policy.</summary>
public static class ManualScenarioWindowPolicy
{
    /// <summary>Names the band a manual scenario window score falls in.</summary>
    public static string ClassifyManualScenarioWindow(double score)
    {
        if (score < 14.6)
        {
            return "critical";
        }
        if (score < 66.2)
        {
            return "minimal";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualScenarioWindow()
    {
        var policy = Retry.DefaultPolicy("manual-scenario-window");
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
