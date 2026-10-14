namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestOverrideEngineChecks
{
    public static void LeaveRequestOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestOverrideEngine.DefaultLeaveRequestOverrideEngine(Flags.NewRegistry());
        var item = LeaveRequestOverride.NewLeaveRequestOverride("code-590", "label-209");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestOverrideEngineKeepsResultsWithinBounds();
    }
}
