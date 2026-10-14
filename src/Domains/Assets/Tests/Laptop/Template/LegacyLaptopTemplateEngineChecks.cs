namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyLaptopTemplateEngineChecks
{
    public static void LegacyLaptopTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyLaptopTemplateEngine.DefaultLegacyLaptopTemplateEngine();
        var item = LegacyLaptopTemplate.NewLegacyLaptopTemplate("code-988", "title-380");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyLaptopTemplateEngineKeepsResultsWithinBounds();
    }
}
