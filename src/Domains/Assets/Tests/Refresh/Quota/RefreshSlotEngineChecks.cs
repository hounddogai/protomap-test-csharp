namespace Acme.Workforce.Assets.Tests.Refresh.Quota;

using Acme.Workforce.Assets.Refresh.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RefreshSlotEngineChecks
{
    public static void RefreshSlotEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshSlotEngine.DefaultRefreshSlotEngine(Flags.NewRegistry());
        var item = RefreshSlot.NewRefreshSlot("code-817", "name-217");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshSlotEngineKeepsResultsWithinBounds();
    }
}
