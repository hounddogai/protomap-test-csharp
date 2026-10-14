namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DefaultPolicyAssignmentEngineChecks
{
    public static void DefaultPolicyAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPolicyAssignmentEngine.DefaultDefaultPolicyAssignmentEngine(Flags.NewRegistry());
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("code-296", "name-825");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPolicyAssignmentEngineKeepsResultsWithinBounds();
    }
}
