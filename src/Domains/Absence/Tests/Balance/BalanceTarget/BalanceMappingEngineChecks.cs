namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class BalanceMappingEngineChecks
{
    public static void BalanceMappingEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceMappingEngine.DefaultBalanceMappingEngine();
        var item = BalanceMapping.NewBalanceMapping("code-102", "title-283");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceMappingEngineKeepsResultsWithinBounds();
    }
}
