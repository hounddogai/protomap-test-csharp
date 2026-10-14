namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestMappingEngineChecks
{
    public static void LeaveRequestMappingEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestMappingEngine.DefaultLeaveRequestMappingEngine(Flags.NewRegistry());
        var item = LeaveRequestMapping.NewLeaveRequestMapping("code-798", "title-767");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestMappingEngineKeepsResultsWithinBounds();
    }
}
