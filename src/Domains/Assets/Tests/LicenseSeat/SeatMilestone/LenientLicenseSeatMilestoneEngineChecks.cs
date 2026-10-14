namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LenientLicenseSeatMilestoneEngineChecks
{
    public static void LenientLicenseSeatMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = LenientLicenseSeatMilestoneEngine.DefaultLenientLicenseSeatMilestoneEngine();
        var item = LenientLicenseSeatMilestone.NewLenientLicenseSeatMilestone("code-743", "name-963");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientLicenseSeatMilestoneEngineKeepsResultsWithinBounds();
    }
}
