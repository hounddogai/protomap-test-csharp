namespace Acme.Workforce.Compensation.Tests.PayBand.Tier;

using Acme.Workforce.Compensation.PayBand.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LegacyPayBandRollupEngineChecks
{
    public static void LegacyPayBandRollupEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPayBandRollupEngine.DefaultLegacyPayBandRollupEngine();
        var item = LegacyPayBandRollup.NewLegacyPayBandRollup("code-311", "name-144");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPayBandRollupEngineKeepsResultsWithinBounds();
    }
}
