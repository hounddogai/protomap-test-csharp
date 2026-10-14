namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatCycle;

using Acme.Workforce.Assets.LicenseSeat.SeatCycle;

/// <summary>Checks of seat cycle code, run by the project's runner.</summary>
public static class LicenseSeatDigestEngineChecks
{
    public static void LicenseSeatDigestEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatDigestEngine.DefaultLicenseSeatDigestEngine();
        var item = LicenseSeatDigest.NewLicenseSeatDigest("code-898", "name-414");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatDigestEngineKeepsResultsWithinBounds();
    }
}
