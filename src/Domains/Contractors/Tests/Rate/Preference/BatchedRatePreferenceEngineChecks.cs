namespace Acme.Workforce.Contractors.Tests.Rate.Preference;

using Acme.Workforce.Contractors.Rate.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class BatchedRatePreferenceEngineChecks
{
    public static void BatchedRatePreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedRatePreferenceEngine.DefaultBatchedRatePreferenceEngine();
        var item = BatchedRatePreference.NewBatchedRatePreference("code-914", "title-650");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedRatePreferenceEngineKeepsResultsWithinBounds();
    }
}
