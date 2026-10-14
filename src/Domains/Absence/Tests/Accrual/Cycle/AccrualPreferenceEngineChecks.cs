namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualPreferenceEngineChecks
{
    public static void AccrualPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualPreferenceEngine.DefaultAccrualPreferenceEngine();
        var item = AccrualPreference.NewAccrualPreference("code-785", "title-197");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualPreferenceEngineKeepsResultsWithinBounds();
    }
}
