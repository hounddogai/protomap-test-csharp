namespace Acme.Workforce.Assets.Tests.Assignment.Batch;

using Acme.Workforce.Assets.Assignment.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryAssignmentTierEngineChecks
{
    public static void PrimaryAssignmentTierEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryAssignmentTierEngine.DefaultPrimaryAssignmentTierEngine(Flags.NewRegistry());
        var item = PrimaryAssignmentTier.NewPrimaryAssignmentTier("code-75", "label-615");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryAssignmentTierEngineKeepsResultsWithinBounds();
    }
}
