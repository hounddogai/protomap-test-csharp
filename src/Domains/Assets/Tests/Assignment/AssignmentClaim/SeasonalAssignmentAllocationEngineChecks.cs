namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentClaim;

using Acme.Workforce.Assets.Assignment.AssignmentClaim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment claim code, run by the project's runner.</summary>
public static class SeasonalAssignmentAllocationEngineChecks
{
    public static void SeasonalAssignmentAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalAssignmentAllocationEngine.DefaultSeasonalAssignmentAllocationEngine(Flags.NewRegistry());
        var item = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation("code-641", "name-40");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalAssignmentAllocationEngineKeepsResultsWithinBounds();
    }
}
