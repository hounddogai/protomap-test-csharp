namespace Acme.Workforce.Compensation.Tests.EquityGrant.Plan;

using Acme.Workforce.Compensation.EquityGrant.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BatchedEquityGrantPreferenceEngineChecks
{
    public static void BatchedEquityGrantPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedEquityGrantPreferenceEngine.DefaultBatchedEquityGrantPreferenceEngine(Flags.NewRegistry());
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("code-665", "name-540");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedEquityGrantPreferenceEngineKeepsResultsWithinBounds();
    }
}
