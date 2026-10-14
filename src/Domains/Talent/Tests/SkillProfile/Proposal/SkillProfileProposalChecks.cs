namespace Acme.Workforce.Talent.Tests.SkillProfile.Proposal;

using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SkillProfileProposalChecks
{
    public static void SkillProfileProposalStartsInRequested()
    {
        var item = SkillProfileProposal.NewSkillProfileProposal("code-434", "label-269");
        Check.Equal(item.Status, SkillProfileProposalStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileProposalMovesToExpired()
    {
        var item = SkillProfileProposal.NewSkillProfileProposal("code-434", "label-269");
        try
        {
            item.MoveTo(SkillProfileProposalStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileProposalStatus.Expired, "status");
    }

    public static void SkillProfileProposalRejectsMovesFromExpired()
    {
        var item = SkillProfileProposal.NewSkillProfileProposal("code-434", "label-269");
        item.Status = SkillProfileProposalStatus.Expired;
        Check.Fails(() => item.MoveTo(SkillProfileProposalStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SkillProfileProposalStartsInRequested();
        SkillProfileProposalMovesToExpired();
        SkillProfileProposalRejectsMovesFromExpired();
    }
}
