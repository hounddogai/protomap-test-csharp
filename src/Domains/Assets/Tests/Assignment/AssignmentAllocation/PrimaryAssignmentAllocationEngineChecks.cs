namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentAllocation;

using Acme.Workforce.Assets.Assignment.AssignmentAllocation;

/// <summary>Checks of assignment allocation code, run by the project's runner.</summary>
public static class PrimaryAssignmentAllocationEngineChecks
{
    public static void PrimaryAssignmentAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryAssignmentAllocationEngine.DefaultPrimaryAssignmentAllocationEngine();
        var item = PrimaryAssignmentAllocation.NewPrimaryAssignmentAllocation("code-224", "name-88");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryAssignmentAllocationEngineKeepsResultsWithinBounds();
    }
}
