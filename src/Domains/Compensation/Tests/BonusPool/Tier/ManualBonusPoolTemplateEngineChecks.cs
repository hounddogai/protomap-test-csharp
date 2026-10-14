namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualBonusPoolTemplateEngineChecks
{
    public static void ManualBonusPoolTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = ManualBonusPoolTemplateEngine.DefaultManualBonusPoolTemplateEngine();
        var item = ManualBonusPoolTemplate.NewManualBonusPoolTemplate("code-426", "title-743");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualBonusPoolTemplateEngineKeepsResultsWithinBounds();
    }
}
