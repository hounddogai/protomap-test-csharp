namespace Acme.Workforce.Contractors.Tests.Offboarding.Batch;

using Acme.Workforce.Contractors.Offboarding.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ScheduledOffboardingBatchEngineChecks
{
    public static void ScheduledOffboardingBatchEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledOffboardingBatchEngine.DefaultScheduledOffboardingBatchEngine(Flags.NewRegistry());
        var item = ScheduledOffboardingBatch.NewScheduledOffboardingBatch("code-893", "name-823");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledOffboardingBatchEngineKeepsResultsWithinBounds();
    }
}
