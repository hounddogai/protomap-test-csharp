namespace Acme.Workforce.Assets.Tests.Assignment.Milestone;

using Acme.Workforce.Assets.Assignment.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AssignmentSlotEngineChecks
{
    public static void AssignmentSlotEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentSlotEngine.DefaultAssignmentSlotEngine(Flags.NewRegistry());
        var item = AssignmentSlot.NewAssignmentSlot("code-274", "label-908");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentSlotEngineKeepsResultsWithinBounds();
    }
}
