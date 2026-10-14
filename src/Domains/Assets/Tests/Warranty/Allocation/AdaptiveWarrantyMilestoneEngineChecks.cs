namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdaptiveWarrantyMilestoneEngineChecks
{
    public static void AdaptiveWarrantyMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveWarrantyMilestoneEngine.DefaultAdaptiveWarrantyMilestoneEngine(Flags.NewRegistry());
        var item = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone("code-140", "label-164");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyMilestoneEngineKeepsResultsWithinBounds();
    }
}
