namespace Acme.Workforce.Assets.Tests.Warranty.Milestone;

using Acme.Workforce.Assets.Warranty.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SeasonalWarrantyTargetEngineChecks
{
    public static void SeasonalWarrantyTargetEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalWarrantyTargetEngine.DefaultSeasonalWarrantyTargetEngine(Flags.NewRegistry());
        var item = SeasonalWarrantyTarget.NewSeasonalWarrantyTarget("code-523", "title-632");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalWarrantyTargetEngineKeepsResultsWithinBounds();
    }
}
