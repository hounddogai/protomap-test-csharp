namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RefreshEntryEngineChecks
{
    public static void RefreshEntryEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshEntryEngine.DefaultRefreshEntryEngine(Flags.NewRegistry());
        var item = RefreshEntry.NewRefreshEntry("code-827", "label-149");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshEntryEngineKeepsResultsWithinBounds();
    }
}
