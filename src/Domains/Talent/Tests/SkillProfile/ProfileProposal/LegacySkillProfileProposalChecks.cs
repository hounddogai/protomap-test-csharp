namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileProposal;

using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Checks of profile proposal code, run by the project's runner.</summary>
public static class LegacySkillProfileProposalChecks
{
    public static void LegacySkillProfileProposalStartsInProposed()
    {
        var item = LegacySkillProfileProposal.NewLegacySkillProfileProposal("code-616", "title-772");
        Check.Equal(item.Status, LegacySkillProfileProposalStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacySkillProfileProposalMovesToApproved()
    {
        var item = LegacySkillProfileProposal.NewLegacySkillProfileProposal("code-616", "title-772");
        try
        {
            item.MoveTo(LegacySkillProfileProposalStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacySkillProfileProposalStatus.Approved, "status");
    }

    public static void LegacySkillProfileProposalRejectsMovesFromWithdrawn()
    {
        var item = LegacySkillProfileProposal.NewLegacySkillProfileProposal("code-616", "title-772");
        item.Status = LegacySkillProfileProposalStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacySkillProfileProposalStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacySkillProfileProposalStartsInProposed();
        LegacySkillProfileProposalMovesToApproved();
        LegacySkillProfileProposalRejectsMovesFromWithdrawn();
    }
}
