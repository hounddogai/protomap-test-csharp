namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatScoreEngineChecks
{
    public static void LicenseSeatScoreEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatScoreEngine.DefaultLicenseSeatScoreEngine();
        var item = LicenseSeatScore.NewLicenseSeatScore("code-365", "title-509");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatScoreEngineKeepsResultsWithinBounds();
    }
}
