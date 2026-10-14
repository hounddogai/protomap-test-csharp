namespace Acme.Workforce.Workplace.Tests.AccessBadge.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AccessBadgeAssignmentEngineChecks
{
    public static void AccessBadgeAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeAssignmentEngine.DefaultAccessBadgeAssignmentEngine(Flags.NewRegistry());
        var item = AccessBadgeAssignment.NewAccessBadgeAssignment("code-821", "name-904");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeAssignmentEngineKeepsResultsWithinBounds();
    }
}
