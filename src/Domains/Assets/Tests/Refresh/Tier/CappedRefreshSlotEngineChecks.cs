namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class CappedRefreshSlotEngineChecks
{
    public static void CappedRefreshSlotEngineKeepsResultsWithinBounds()
    {
        var engine = CappedRefreshSlotEngine.DefaultCappedRefreshSlotEngine();
        var item = CappedRefreshSlot.NewCappedRefreshSlot("code-581", "label-412");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedRefreshSlotEngineKeepsResultsWithinBounds();
    }
}
