namespace Acme.Workforce.Assets.Tests.Laptop.Mapping;

using Acme.Workforce.Assets.Laptop.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultLaptopWindowEngineChecks
{
    public static void DefaultLaptopWindowEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultLaptopWindowEngine.DefaultDefaultLaptopWindowEngine(Flags.NewRegistry());
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("code-130", "label-387");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultLaptopWindowEngineKeepsResultsWithinBounds();
    }
}
