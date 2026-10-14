namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class RollingAccessBadgeProfileEngineChecks
{
    public static void RollingAccessBadgeProfileEngineKeepsResultsWithinBounds()
    {
        var engine = RollingAccessBadgeProfileEngine.DefaultRollingAccessBadgeProfileEngine(Flags.NewRegistry());
        var item = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile("code-54", "label-151");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingAccessBadgeProfileEngineKeepsResultsWithinBounds();
    }
}
