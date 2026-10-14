namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceBaselineEngineChecks
{
    public static void AdaptiveBalanceBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveBalanceBaselineEngine.DefaultAdaptiveBalanceBaselineEngine();
        var item = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline("code-331", "title-179");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveBalanceBaselineEngineKeepsResultsWithinBounds();
    }
}
