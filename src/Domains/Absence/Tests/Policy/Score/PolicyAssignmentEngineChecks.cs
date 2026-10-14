namespace Acme.Workforce.Absence.Tests.Policy.Score;

using Acme.Workforce.Absence.Policy.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class PolicyAssignmentEngineChecks
{
    public static void PolicyAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAssignmentEngine.DefaultPolicyAssignmentEngine(Flags.NewRegistry());
        var item = PolicyAssignment.NewPolicyAssignment("code-258", "title-612");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAssignmentEngineKeepsResultsWithinBounds();
    }
}
