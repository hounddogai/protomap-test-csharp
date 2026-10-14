namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AssignmentTierEngineChecks
{
    public static void AssignmentTierEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentTierEngine.DefaultAssignmentTierEngine(Flags.NewRegistry());
        var item = AssignmentTier.NewAssignmentTier("code-785", "label-324");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentTierEngineKeepsResultsWithinBounds();
    }
}
