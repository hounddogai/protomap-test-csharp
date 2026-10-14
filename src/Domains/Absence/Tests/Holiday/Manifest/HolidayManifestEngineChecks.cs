namespace Acme.Workforce.Absence.Tests.Holiday.Manifest;

using Acme.Workforce.Absence.Holiday.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class HolidayManifestEngineChecks
{
    public static void HolidayManifestEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayManifestEngine.DefaultHolidayManifestEngine(Flags.NewRegistry());
        var item = HolidayManifest.NewHolidayManifest("code-850", "name-687");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayManifestEngineKeepsResultsWithinBounds();
    }
}
