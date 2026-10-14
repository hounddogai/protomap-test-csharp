namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RollingBonusPoolTemplateEngineChecks
{
    public static void RollingBonusPoolTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = RollingBonusPoolTemplateEngine.DefaultRollingBonusPoolTemplateEngine(Flags.NewRegistry());
        var item = RollingBonusPoolTemplate.NewRollingBonusPoolTemplate("code-995", "label-141");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingBonusPoolTemplateEngineKeepsResultsWithinBounds();
    }
}
