namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class RollingAccessBadgeRequestEngineChecks
{
    public static void RollingAccessBadgeRequestEngineKeepsResultsWithinBounds()
    {
        var engine = RollingAccessBadgeRequestEngine.DefaultRollingAccessBadgeRequestEngine(Flags.NewRegistry());
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("code-645", "label-901");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingAccessBadgeRequestEngineKeepsResultsWithinBounds();
    }
}
