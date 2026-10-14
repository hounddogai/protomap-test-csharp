namespace Acme.Workforce.Assets.Tests.Assignment.Milestone;

using Acme.Workforce.Assets.Assignment.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AssignmentMilestoneEngineChecks
{
    public static void AssignmentMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentMilestoneEngine.DefaultAssignmentMilestoneEngine(Flags.NewRegistry());
        var item = AssignmentMilestone.NewAssignmentMilestone("code-989", "label-926");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentMilestoneEngineKeepsResultsWithinBounds();
    }
}
