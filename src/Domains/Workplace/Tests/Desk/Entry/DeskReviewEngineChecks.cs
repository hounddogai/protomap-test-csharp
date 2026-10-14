namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskReviewEngineChecks
{
    public static void DeskReviewEngineKeepsResultsWithinBounds()
    {
        var engine = DeskReviewEngine.DefaultDeskReviewEngine(Flags.NewRegistry());
        var item = DeskReview.NewDeskReview("code-550", "name-414");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskReviewEngineKeepsResultsWithinBounds();
    }
}
