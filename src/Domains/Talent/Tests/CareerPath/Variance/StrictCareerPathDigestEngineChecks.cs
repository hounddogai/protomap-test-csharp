namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictCareerPathDigestEngineChecks
{
    public static void StrictCareerPathDigestEngineKeepsResultsWithinBounds()
    {
        var engine = StrictCareerPathDigestEngine.DefaultStrictCareerPathDigestEngine(Flags.NewRegistry());
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("code-126", "name-233");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictCareerPathDigestEngineKeepsResultsWithinBounds();
    }
}
