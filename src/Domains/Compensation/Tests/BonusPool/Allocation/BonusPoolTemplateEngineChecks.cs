namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BonusPoolTemplateEngineChecks
{
    public static void BonusPoolTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolTemplateEngine.DefaultBonusPoolTemplateEngine(Flags.NewRegistry());
        var item = BonusPoolTemplate.NewBonusPoolTemplate("code-381", "title-954");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolTemplateEngineKeepsResultsWithinBounds();
    }
}
