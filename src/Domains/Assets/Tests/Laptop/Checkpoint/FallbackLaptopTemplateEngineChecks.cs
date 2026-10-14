namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FallbackLaptopTemplateEngineChecks
{
    public static void FallbackLaptopTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackLaptopTemplateEngine.DefaultFallbackLaptopTemplateEngine();
        var item = FallbackLaptopTemplate.NewFallbackLaptopTemplate("code-295", "label-591");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackLaptopTemplateEngineKeepsResultsWithinBounds();
    }
}
