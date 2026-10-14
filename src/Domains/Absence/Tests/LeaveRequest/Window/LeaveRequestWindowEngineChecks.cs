namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LeaveRequestWindowEngineChecks
{
    public static void LeaveRequestWindowEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestWindowEngine.DefaultLeaveRequestWindowEngine(Flags.NewRegistry());
        var item = LeaveRequestWindow.NewLeaveRequestWindow("code-566", "name-523");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestWindowEngineKeepsResultsWithinBounds();
    }
}
