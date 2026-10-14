namespace Acme.Workforce.Assets.Tests.Assignment.Agreement;

using Acme.Workforce.Assets.Assignment.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssignmentPolicyEngineChecks
{
    public static void AssignmentPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentPolicyEngine.DefaultAssignmentPolicyEngine(Flags.NewRegistry());
        var item = AssignmentPolicy.NewAssignmentPolicy("code-151", "title-788");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentPolicyEngineKeepsResultsWithinBounds();
    }
}
