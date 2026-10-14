namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WeightedGarnishmentReviewEngineChecks
{
    public static void WeightedGarnishmentReviewEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedGarnishmentReviewEngine.DefaultWeightedGarnishmentReviewEngine(Flags.NewRegistry());
        var item = WeightedGarnishmentReview.NewWeightedGarnishmentReview("code-894", "title-599");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedGarnishmentReviewEngineKeepsResultsWithinBounds();
    }
}
