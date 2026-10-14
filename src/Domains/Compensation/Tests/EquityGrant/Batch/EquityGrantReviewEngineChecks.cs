namespace Acme.Workforce.Compensation.Tests.EquityGrant.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class EquityGrantReviewEngineChecks
{
    public static void EquityGrantReviewEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantReviewEngine.DefaultEquityGrantReviewEngine(Flags.NewRegistry());
        var item = EquityGrantReview.NewEquityGrantReview("code-48", "name-281");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantReviewEngineKeepsResultsWithinBounds();
    }
}
