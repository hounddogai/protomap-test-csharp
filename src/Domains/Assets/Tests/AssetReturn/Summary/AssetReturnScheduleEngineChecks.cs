namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnScheduleEngineChecks
{
    public static void AssetReturnScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnScheduleEngine.DefaultAssetReturnScheduleEngine(Flags.NewRegistry());
        var item = AssetReturnSchedule.NewAssetReturnSchedule("code-482", "title-836");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnScheduleEngineKeepsResultsWithinBounds();
    }
}
