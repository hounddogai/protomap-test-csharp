namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantAssignment;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of grant assignment code, run by the project's runner.</summary>
public static class GlobalEquityGrantAssignmentEngineChecks
{
    public static void GlobalEquityGrantAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalEquityGrantAssignmentEngine.DefaultGlobalEquityGrantAssignmentEngine(Flags.NewRegistry());
        var item = GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment("code-875", "label-685");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalEquityGrantAssignmentEngineKeepsResultsWithinBounds();
    }
}
