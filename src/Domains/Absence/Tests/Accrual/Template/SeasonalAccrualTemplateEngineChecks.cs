namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SeasonalAccrualTemplateEngineChecks
{
    public static void SeasonalAccrualTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalAccrualTemplateEngine.DefaultSeasonalAccrualTemplateEngine();
        var item = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate("code-552", "name-482");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalAccrualTemplateEngineKeepsResultsWithinBounds();
    }
}
