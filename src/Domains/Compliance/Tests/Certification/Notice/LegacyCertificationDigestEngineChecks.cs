namespace Acme.Workforce.Compliance.Tests.Certification.Notice;

using Acme.Workforce.Compliance.Certification.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LegacyCertificationDigestEngineChecks
{
    public static void LegacyCertificationDigestEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyCertificationDigestEngine.DefaultLegacyCertificationDigestEngine(Flags.NewRegistry());
        var item = LegacyCertificationDigest.NewLegacyCertificationDigest("code-70", "name-566");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyCertificationDigestEngineKeepsResultsWithinBounds();
    }
}
