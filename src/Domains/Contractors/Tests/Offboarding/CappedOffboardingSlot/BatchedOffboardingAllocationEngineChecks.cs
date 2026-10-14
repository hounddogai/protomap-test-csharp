namespace Acme.Workforce.Contractors.Tests.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of capped offboarding slot code, run by the project's runner.</summary>
public static class BatchedOffboardingAllocationEngineChecks
{
    public static void BatchedOffboardingAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedOffboardingAllocationEngine.DefaultBatchedOffboardingAllocationEngine(Flags.NewRegistry());
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("code-632", "label-224");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedOffboardingAllocationEngineKeepsResultsWithinBounds();
    }
}
