namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalBonusPoolOverrideEngineChecks
{
    public static void RegionalBonusPoolOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalBonusPoolOverrideEngine.DefaultRegionalBonusPoolOverrideEngine(Flags.NewRegistry());
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("code-971", "label-9");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalBonusPoolOverrideEngineKeepsResultsWithinBounds();
    }
}
