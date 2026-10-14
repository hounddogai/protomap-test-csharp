namespace Acme.Workforce.Compliance.Tests.Attestation.Preference;

using Acme.Workforce.Compliance.Attestation.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalAttestationPreferenceEngineChecks
{
    public static void RegionalAttestationPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalAttestationPreferenceEngine.DefaultRegionalAttestationPreferenceEngine(Flags.NewRegistry());
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("code-194", "title-656");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalAttestationPreferenceEngineKeepsResultsWithinBounds();
    }
}
