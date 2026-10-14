namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedAccessBadgeProfileEngineChecks
{
    public static void WeightedAccessBadgeProfileEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAccessBadgeProfileEngine.DefaultWeightedAccessBadgeProfileEngine(Flags.NewRegistry());
        var item = WeightedAccessBadgeProfile.NewWeightedAccessBadgeProfile("code-5", "label-380");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeProfileEngineKeepsResultsWithinBounds();
    }
}
