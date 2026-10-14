namespace Acme.Workforce.Absence.Tests.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SeasonalAccrualDigestEngineChecks
{
    public static void SeasonalAccrualDigestEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalAccrualDigestEngine.DefaultSeasonalAccrualDigestEngine();
        var item = SeasonalAccrualDigest.NewSeasonalAccrualDigest("code-94", "name-598");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalAccrualDigestEngineKeepsResultsWithinBounds();
    }
}
