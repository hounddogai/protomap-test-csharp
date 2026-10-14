namespace Acme.Workforce.Payrollops.Tests.PayRun.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedPayRunPreferenceEngineChecks
{
    public static void WeightedPayRunPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedPayRunPreferenceEngine.DefaultWeightedPayRunPreferenceEngine(Flags.NewRegistry());
        var item = WeightedPayRunPreference.NewWeightedPayRunPreference("code-676", "title-466");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedPayRunPreferenceEngineKeepsResultsWithinBounds();
    }
}
