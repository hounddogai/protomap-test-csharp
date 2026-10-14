namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictPayBandPreferenceEngineChecks
{
    public static void StrictPayBandPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPayBandPreferenceEngine.DefaultStrictPayBandPreferenceEngine(Flags.NewRegistry());
        var item = StrictPayBandPreference.NewStrictPayBandPreference("code-527", "name-204");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPayBandPreferenceEngineKeepsResultsWithinBounds();
    }
}
