namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RollingLaptopMappingEngineChecks
{
    public static void RollingLaptopMappingEngineKeepsResultsWithinBounds()
    {
        var engine = RollingLaptopMappingEngine.DefaultRollingLaptopMappingEngine(Flags.NewRegistry());
        var item = RollingLaptopMapping.NewRollingLaptopMapping("code-425", "name-878");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingLaptopMappingEngineKeepsResultsWithinBounds();
    }
}
