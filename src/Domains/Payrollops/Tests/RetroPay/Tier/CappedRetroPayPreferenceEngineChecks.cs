namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class CappedRetroPayPreferenceEngineChecks
{
    public static void CappedRetroPayPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = CappedRetroPayPreferenceEngine.DefaultCappedRetroPayPreferenceEngine();
        var item = CappedRetroPayPreference.NewCappedRetroPayPreference("code-32", "label-406");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedRetroPayPreferenceEngineKeepsResultsWithinBounds();
    }
}
