namespace Acme.Workforce.Assets.Tests.LicenseSeat.Template;

using Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LicenseSeatTemplateEngineChecks
{
    public static void LicenseSeatTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatTemplateEngine.DefaultLicenseSeatTemplateEngine();
        var item = LicenseSeatTemplate.NewLicenseSeatTemplate("code-558", "name-666");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatTemplateEngineKeepsResultsWithinBounds();
    }
}
