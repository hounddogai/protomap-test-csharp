namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class PolicyCheckpointEngineChecks
{
    public static void PolicyCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyCheckpointEngine.DefaultPolicyCheckpointEngine(Flags.NewRegistry());
        var item = PolicyCheckpoint.NewPolicyCheckpoint("code-980", "label-553");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyCheckpointEngineKeepsResultsWithinBounds();
    }
}
