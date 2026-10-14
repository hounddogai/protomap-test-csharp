namespace Acme.Workforce.Absence.Tests.Balance.Request;

using Acme.Workforce.Absence.Balance.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class BalanceRequestEngineChecks
{
    public static void BalanceRequestEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceRequestEngine.DefaultBalanceRequestEngine();
        var item = BalanceRequest.NewBalanceRequest("code-970", "name-361");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceRequestEngineKeepsResultsWithinBounds();
    }
}
