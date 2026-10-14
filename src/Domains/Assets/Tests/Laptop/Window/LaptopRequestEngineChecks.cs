namespace Acme.Workforce.Assets.Tests.Laptop.Window;

using Acme.Workforce.Assets.Laptop.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LaptopRequestEngineChecks
{
    public static void LaptopRequestEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopRequestEngine.DefaultLaptopRequestEngine();
        var item = LaptopRequest.NewLaptopRequest("code-437", "label-488");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopRequestEngineKeepsResultsWithinBounds();
    }
}
