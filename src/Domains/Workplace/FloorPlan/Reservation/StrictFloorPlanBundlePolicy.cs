namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict floor plan bundle policy.</summary>
public static class StrictFloorPlanBundlePolicy
{
    public static string StrictFloorPlanBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictFloorPlanBundle()
    {
        var policy = Retry.DefaultPolicy("strict-floor-plan-bundle");
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

    /// <summary>Puts a strict floor plan bundle code in its canonical SFPB- form.</summary>
    public static string NormalizeStrictFloorPlanBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int StrictFloorPlanBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
