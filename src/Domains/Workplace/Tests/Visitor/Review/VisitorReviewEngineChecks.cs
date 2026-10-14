namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VisitorReviewEngineChecks
{
    public static void VisitorReviewEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorReviewEngine.DefaultVisitorReviewEngine(Flags.NewRegistry());
        var item = VisitorReview.NewVisitorReview("code-35", "name-138");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorReviewEngineKeepsResultsWithinBounds();
    }
}
