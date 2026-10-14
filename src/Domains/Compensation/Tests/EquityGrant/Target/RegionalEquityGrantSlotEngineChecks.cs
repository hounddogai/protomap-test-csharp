namespace Acme.Workforce.Compensation.Tests.EquityGrant.Target;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class RegionalEquityGrantSlotEngineChecks
{
    public static void RegionalEquityGrantSlotEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalEquityGrantSlotEngine.DefaultRegionalEquityGrantSlotEngine();
        var item = RegionalEquityGrantSlot.NewRegionalEquityGrantSlot("code-449", "label-115");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalEquityGrantSlotEngineKeepsResultsWithinBounds();
    }
}
