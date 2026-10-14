namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestReviewEngineChecks
{
    public static void SeasonalLeaveRequestReviewEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalLeaveRequestReviewEngine.DefaultSeasonalLeaveRequestReviewEngine();
        var item = SeasonalLeaveRequestReview.NewSeasonalLeaveRequestReview("code-573", "title-876");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestReviewEngineKeepsResultsWithinBounds();
    }
}
