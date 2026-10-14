namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedOffboardingBaselineEngineChecks
{
    public static void BatchedOffboardingBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedOffboardingBaselineEngine.DefaultBatchedOffboardingBaselineEngine(Flags.NewRegistry());
        var item = BatchedOffboardingBaseline.NewBatchedOffboardingBaseline("code-566", "label-210");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedOffboardingBaselineEngineKeepsResultsWithinBounds();
    }
}
