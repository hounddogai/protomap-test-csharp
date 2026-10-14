namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopCheckpointEngineChecks
{
    public static void LaptopCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopCheckpointEngine.DefaultLaptopCheckpointEngine(Flags.NewRegistry());
        var item = LaptopCheckpoint.NewLaptopCheckpoint("code-641", "label-4");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopCheckpointEngineKeepsResultsWithinBounds();
    }
}
