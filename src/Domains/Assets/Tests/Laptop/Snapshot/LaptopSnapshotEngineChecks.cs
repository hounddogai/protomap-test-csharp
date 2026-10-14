namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LaptopSnapshotEngineChecks
{
    public static void LaptopSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopSnapshotEngine.DefaultLaptopSnapshotEngine();
        var item = LaptopSnapshot.NewLaptopSnapshot("code-291", "title-556");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopSnapshotEngineKeepsResultsWithinBounds();
    }
}
