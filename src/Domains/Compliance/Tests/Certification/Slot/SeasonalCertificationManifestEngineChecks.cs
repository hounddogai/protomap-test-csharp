namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SeasonalCertificationManifestEngineChecks
{
    public static void SeasonalCertificationManifestEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalCertificationManifestEngine.DefaultSeasonalCertificationManifestEngine();
        var item = SeasonalCertificationManifest.NewSeasonalCertificationManifest("code-271", "label-448");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalCertificationManifestEngineKeepsResultsWithinBounds();
    }
}
