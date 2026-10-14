namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictLeaveRequestReviewEngineChecks
{
    public static void StrictLeaveRequestReviewEngineKeepsResultsWithinBounds()
    {
        var engine = StrictLeaveRequestReviewEngine.DefaultStrictLeaveRequestReviewEngine();
        var item = StrictLeaveRequestReview.NewStrictLeaveRequestReview("code-718", "title-546");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictLeaveRequestReviewEngineKeepsResultsWithinBounds();
    }
}
