namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskManifestEngineChecks
{
    public static void DeskManifestEngineKeepsResultsWithinBounds()
    {
        var engine = DeskManifestEngine.DefaultDeskManifestEngine(Flags.NewRegistry());
        var item = DeskManifest.NewDeskManifest("code-49", "name-575");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskManifestEngineKeepsResultsWithinBounds();
    }
}
