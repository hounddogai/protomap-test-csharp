namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopReservationEngineChecks
{
    public static void LaptopReservationEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopReservationEngine.DefaultLaptopReservationEngine();
        var item = LaptopReservation.NewLaptopReservation("code-671", "label-661");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopReservationEngineKeepsResultsWithinBounds();
    }
}
