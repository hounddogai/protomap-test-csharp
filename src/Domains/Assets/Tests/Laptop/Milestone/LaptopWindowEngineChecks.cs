namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopWindowEngineChecks
{
    public static void LaptopWindowEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopWindowEngine.DefaultLaptopWindowEngine(Flags.NewRegistry());
        var item = LaptopWindow.NewLaptopWindow("code-917", "name-418");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopWindowEngineKeepsResultsWithinBounds();
    }
}
