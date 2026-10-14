namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual action plan preference policy.</summary>
public static class ManualActionPlanPreferencePolicy
{
    /// <summary>Puts a manual action plan preference code in its canonical MAPP- form.</summary>
    public static string NormalizeManualActionPlanPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MAPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MAPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ManualActionPlanPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a manual action plan preference score falls in.</summary>
    public static string ClassifyManualActionPlanPreference(double score)
    {
        if (score < 68.0)
        {
            return "medium";
        }
        if (score < 69.8)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualActionPlanPreference()
    {
        var policy = Retry.DefaultPolicy("manual-action-plan-preference");
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
}
