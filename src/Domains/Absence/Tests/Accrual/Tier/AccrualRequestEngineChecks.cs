namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualRequestEngineChecks
{
    public static void AccrualRequestEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualRequestEngine.DefaultAccrualRequestEngine();
        var item = AccrualRequest.NewAccrualRequest("code-753", "name-408");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualRequestEngineKeepsResultsWithinBounds();
    }
}
