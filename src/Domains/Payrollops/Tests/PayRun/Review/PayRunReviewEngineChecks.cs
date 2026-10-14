namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunReviewEngineChecks
{
    public static void PayRunReviewEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunReviewEngine.DefaultPayRunReviewEngine(Flags.NewRegistry());
        var item = PayRunReview.NewPayRunReview("code-995", "title-392");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunReviewEngineKeepsResultsWithinBounds();
    }
}
