namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceVarianceEngineChecks
{
    public static void BalanceVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceVarianceEngine.DefaultBalanceVarianceEngine();
        var item = BalanceVariance.NewBalanceVariance("code-848", "name-2");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceVarianceEngineKeepsResultsWithinBounds();
    }
}
