namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TrainingDueAllocationEngineChecks
{
    public static void TrainingDueAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueAllocationEngine.DefaultTrainingDueAllocationEngine(Flags.NewRegistry());
        var item = TrainingDueAllocation.NewTrainingDueAllocation("code-959", "title-610");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueAllocationEngineKeepsResultsWithinBounds();
    }
}
