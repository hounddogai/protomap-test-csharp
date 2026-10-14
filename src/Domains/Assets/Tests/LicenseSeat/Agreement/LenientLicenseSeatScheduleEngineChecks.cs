namespace Acme.Workforce.Assets.Tests.LicenseSeat.Agreement;

using Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientLicenseSeatScheduleEngineChecks
{
    public static void LenientLicenseSeatScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = LenientLicenseSeatScheduleEngine.DefaultLenientLicenseSeatScheduleEngine();
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("code-531", "label-125");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientLicenseSeatScheduleEngineKeepsResultsWithinBounds();
    }
}
