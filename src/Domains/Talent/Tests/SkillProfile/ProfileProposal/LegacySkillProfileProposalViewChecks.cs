namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileProposal;

using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Checks of profile proposal code, run by the project's runner.</summary>
public static class LegacySkillProfileProposalViewChecks
{
    public static void LegacySkillProfileProposalViewShowsTheStatusInCapitals()
    {
        var item = LegacySkillProfileProposal.NewLegacySkillProfileProposal("code-616", "title-772");
        var view = LegacySkillProfileProposalView.ToLegacySkillProfileProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacySkillProfileProposalView.ToLegacySkillProfileProposalViews(new List<LegacySkillProfileProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacySkillProfileProposalViewShowsTheStatusInCapitals();
    }
}
