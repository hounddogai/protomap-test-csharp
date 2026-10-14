namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientTimesheetScoreEngineChecks
{
    public static void LenientTimesheetScoreEngineKeepsResultsWithinBounds()
    {
        var engine = LenientTimesheetScoreEngine.DefaultLenientTimesheetScoreEngine();
        var item = LenientTimesheetScore.NewLenientTimesheetScore("code-811", "label-457");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientTimesheetScoreEngineKeepsResultsWithinBounds();
    }
}
