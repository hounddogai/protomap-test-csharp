namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalHolidayCycleEngineChecks
{
    public static void RegionalHolidayCycleEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalHolidayCycleEngine.DefaultRegionalHolidayCycleEngine();
        var item = RegionalHolidayCycle.NewRegionalHolidayCycle("code-686", "name-130");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalHolidayCycleEngineKeepsResultsWithinBounds();
    }
}
