namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualTierEngineChecks
{
    public static void AccrualTierEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualTierEngine.DefaultAccrualTierEngine();
        var item = AccrualTier.NewAccrualTier("code-140", "label-970");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualTierEngineKeepsResultsWithinBounds();
    }
}
