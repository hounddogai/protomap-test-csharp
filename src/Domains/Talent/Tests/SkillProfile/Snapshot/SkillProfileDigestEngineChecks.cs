namespace Acme.Workforce.Talent.Tests.SkillProfile.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SkillProfileDigestEngineChecks
{
    public static void SkillProfileDigestEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileDigestEngine.DefaultSkillProfileDigestEngine(Flags.NewRegistry());
        var item = SkillProfileDigest.NewSkillProfileDigest("code-747", "title-935");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileDigestEngineKeepsResultsWithinBounds();
    }
}
