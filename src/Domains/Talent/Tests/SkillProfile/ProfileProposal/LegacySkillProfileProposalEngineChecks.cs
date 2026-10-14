namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Checks of profile proposal code, run by the project's runner.</summary>
public static class LegacySkillProfileProposalEngineChecks
{
    public static void LegacySkillProfileProposalEngineKeepsResultsWithinBounds()
    {
        var engine = LegacySkillProfileProposalEngine.DefaultLegacySkillProfileProposalEngine(Flags.NewRegistry());
        var item = LegacySkillProfileProposal.NewLegacySkillProfileProposal("code-616", "title-772");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacySkillProfileProposalEngineKeepsResultsWithinBounds();
    }
}
