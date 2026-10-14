namespace Acme.Workforce.Absence.Tests.Policy.Threshold;

using Acme.Workforce.Absence.Policy.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PolicyAllocationEngineChecks
{
    public static void PolicyAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAllocationEngine.DefaultPolicyAllocationEngine();
        var item = PolicyAllocation.NewPolicyAllocation("code-111", "label-447");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAllocationEngineKeepsResultsWithinBounds();
    }
}
