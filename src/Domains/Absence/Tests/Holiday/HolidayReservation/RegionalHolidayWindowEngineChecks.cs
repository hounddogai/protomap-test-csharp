namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class RegionalHolidayWindowEngineChecks
{
    public static void RegionalHolidayWindowEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalHolidayWindowEngine.DefaultRegionalHolidayWindowEngine(Flags.NewRegistry());
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("code-705", "label-811");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalHolidayWindowEngineKeepsResultsWithinBounds();
    }
}
