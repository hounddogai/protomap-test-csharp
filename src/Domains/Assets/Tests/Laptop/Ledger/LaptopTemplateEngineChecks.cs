namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopTemplateEngineChecks
{
    public static void LaptopTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopTemplateEngine.DefaultLaptopTemplateEngine();
        var item = LaptopTemplate.NewLaptopTemplate("code-84", "title-859");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopTemplateEngineKeepsResultsWithinBounds();
    }
}
