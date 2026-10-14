namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentScheduleEngineChecks
{
    public static void AssignmentScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentScheduleEngine.DefaultAssignmentScheduleEngine(Flags.NewRegistry());
        var item = AssignmentSchedule.NewAssignmentSchedule("code-250", "label-924");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentScheduleEngineKeepsResultsWithinBounds();
    }
}
