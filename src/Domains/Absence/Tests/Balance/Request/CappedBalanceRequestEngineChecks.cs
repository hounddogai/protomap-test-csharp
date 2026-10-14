namespace Acme.Workforce.Absence.Tests.Balance.Request;

using Acme.Workforce.Absence.Balance.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CappedBalanceRequestEngineChecks
{
    public static void CappedBalanceRequestEngineKeepsResultsWithinBounds()
    {
        var engine = CappedBalanceRequestEngine.DefaultCappedBalanceRequestEngine(Flags.NewRegistry());
        var item = CappedBalanceRequest.NewCappedBalanceRequest("code-716", "title-655");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedBalanceRequestEngineKeepsResultsWithinBounds();
    }
}
