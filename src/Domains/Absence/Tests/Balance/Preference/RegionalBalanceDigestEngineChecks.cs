namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalBalanceDigestEngineChecks
{
    public static void RegionalBalanceDigestEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalBalanceDigestEngine.DefaultRegionalBalanceDigestEngine(Flags.NewRegistry());
        var item = RegionalBalanceDigest.NewRegionalBalanceDigest("code-294", "name-263");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalBalanceDigestEngineKeepsResultsWithinBounds();
    }
}
