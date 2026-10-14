namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class TieredLeaveRequestOverrideEngineChecks
{
    public static void TieredLeaveRequestOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = TieredLeaveRequestOverrideEngine.DefaultTieredLeaveRequestOverrideEngine(Flags.NewRegistry());
        var item = TieredLeaveRequestOverride.NewTieredLeaveRequestOverride("code-1", "title-262");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredLeaveRequestOverrideEngineKeepsResultsWithinBounds();
    }
}
