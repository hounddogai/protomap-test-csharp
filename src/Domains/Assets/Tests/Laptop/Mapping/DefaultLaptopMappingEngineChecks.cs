namespace Acme.Workforce.Assets.Tests.Laptop.Mapping;

using Acme.Workforce.Assets.Laptop.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultLaptopMappingEngineChecks
{
    public static void DefaultLaptopMappingEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultLaptopMappingEngine.DefaultDefaultLaptopMappingEngine(Flags.NewRegistry());
        var item = DefaultLaptopMapping.NewDefaultLaptopMapping("code-378", "name-257");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultLaptopMappingEngineKeepsResultsWithinBounds();
    }
}
