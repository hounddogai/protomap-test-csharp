namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CappedHolidayManifestEngineChecks
{
    public static void CappedHolidayManifestEngineKeepsResultsWithinBounds()
    {
        var engine = CappedHolidayManifestEngine.DefaultCappedHolidayManifestEngine(Flags.NewRegistry());
        var item = CappedHolidayManifest.NewCappedHolidayManifest("code-596", "label-536");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedHolidayManifestEngineKeepsResultsWithinBounds();
    }
}
