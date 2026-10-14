namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalExceptionPreferenceEngineChecks
{
    public static void RegionalExceptionPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalExceptionPreferenceEngine.DefaultRegionalExceptionPreferenceEngine(Flags.NewRegistry());
        var item = RegionalExceptionPreference.NewRegionalExceptionPreference("code-623", "label-904");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalExceptionPreferenceEngineKeepsResultsWithinBounds();
    }
}
