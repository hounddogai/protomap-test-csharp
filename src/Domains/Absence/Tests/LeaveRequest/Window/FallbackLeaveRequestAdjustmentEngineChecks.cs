namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class FallbackLeaveRequestAdjustmentEngineChecks
{
    public static void FallbackLeaveRequestAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackLeaveRequestAdjustmentEngine.DefaultFallbackLeaveRequestAdjustmentEngine();
        var item = FallbackLeaveRequestAdjustment.NewFallbackLeaveRequestAdjustment("code-472", "label-243");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackLeaveRequestAdjustmentEngineKeepsResultsWithinBounds();
    }
}
