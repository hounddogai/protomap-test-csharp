namespace Acme.Workforce.Payrollops.Tests.Deduction.Score;

using Acme.Workforce.Payrollops.Deduction.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledDeductionEstimateEngineChecks
{
    public static void ScheduledDeductionEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledDeductionEstimateEngine.DefaultScheduledDeductionEstimateEngine();
        var item = ScheduledDeductionEstimate.NewScheduledDeductionEstimate("code-558", "name-565");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledDeductionEstimateEngineKeepsResultsWithinBounds();
    }
}
