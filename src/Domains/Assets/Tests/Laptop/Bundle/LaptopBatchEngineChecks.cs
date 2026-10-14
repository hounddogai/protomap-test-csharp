namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopBatchEngineChecks
{
    public static void LaptopBatchEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopBatchEngine.DefaultLaptopBatchEngine();
        var item = LaptopBatch.NewLaptopBatch("code-885", "name-297");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopBatchEngineKeepsResultsWithinBounds();
    }
}
