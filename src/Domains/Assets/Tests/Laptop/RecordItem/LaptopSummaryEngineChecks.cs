namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopSummaryEngineChecks
{
    public static void LaptopSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopSummaryEngine.DefaultLaptopSummaryEngine();
        var item = LaptopSummary.NewLaptopSummary("code-336", "title-906");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopSummaryEngineKeepsResultsWithinBounds();
    }
}
