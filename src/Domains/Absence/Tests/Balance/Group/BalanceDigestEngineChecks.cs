namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceDigestEngineChecks
{
    public static void BalanceDigestEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceDigestEngine.DefaultBalanceDigestEngine();
        var item = BalanceDigest.NewBalanceDigest("code-503", "title-284");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceDigestEngineKeepsResultsWithinBounds();
    }
}
