namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DeductionReviewEngineChecks
{
    public static void DeductionReviewEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionReviewEngine.DefaultDeductionReviewEngine(Flags.NewRegistry());
        var item = DeductionReview.NewDeductionReview("code-26", "label-973");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionReviewEngineKeepsResultsWithinBounds();
    }
}
