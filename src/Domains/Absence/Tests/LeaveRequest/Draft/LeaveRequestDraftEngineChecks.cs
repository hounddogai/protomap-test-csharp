namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LeaveRequestDraftEngineChecks
{
    public static void LeaveRequestDraftEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestDraftEngine.DefaultLeaveRequestDraftEngine(Flags.NewRegistry());
        var item = LeaveRequestDraft.NewLeaveRequestDraft("code-558", "title-175");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestDraftEngineKeepsResultsWithinBounds();
    }
}
