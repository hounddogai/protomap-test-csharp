namespace Acme.Workforce.Talent.Tests.SkillProfile.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackSkillProfileTierEngineChecks
{
    public static void FallbackSkillProfileTierEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackSkillProfileTierEngine.DefaultFallbackSkillProfileTierEngine(Flags.NewRegistry());
        var item = FallbackSkillProfileTier.NewFallbackSkillProfileTier("code-567", "name-193");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackSkillProfileTierEngineKeepsResultsWithinBounds();
    }
}
