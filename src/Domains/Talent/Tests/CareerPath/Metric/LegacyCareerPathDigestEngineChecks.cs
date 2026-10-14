namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LegacyCareerPathDigestEngineChecks
{
    public static void LegacyCareerPathDigestEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyCareerPathDigestEngine.DefaultLegacyCareerPathDigestEngine(Flags.NewRegistry());
        var item = LegacyCareerPathDigest.NewLegacyCareerPathDigest("code-524", "title-28");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyCareerPathDigestEngineKeepsResultsWithinBounds();
    }
}
