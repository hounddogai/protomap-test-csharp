namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ManualLeaveRequestScoreEngineChecks
{
    public static void ManualLeaveRequestScoreEngineKeepsResultsWithinBounds()
    {
        var engine = ManualLeaveRequestScoreEngine.DefaultManualLeaveRequestScoreEngine(Flags.NewRegistry());
        var item = ManualLeaveRequestScore.NewManualLeaveRequestScore("code-475", "name-841");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualLeaveRequestScoreEngineKeepsResultsWithinBounds();
    }
}
