namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileProposal;

using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Checks of profile proposal code, run by the project's runner.</summary>
public static class SkillProfileSummaryChecks
{
    public static void SkillProfileSummaryStartsInProposed()
    {
        var item = SkillProfileSummary.NewSkillProfileSummary("code-994", "name-547");
        Check.Equal(item.Status, SkillProfileSummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileSummaryMovesToApproved()
    {
        var item = SkillProfileSummary.NewSkillProfileSummary("code-994", "name-547");
        try
        {
            item.MoveTo(SkillProfileSummaryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileSummaryStatus.Approved, "status");
    }

    public static void SkillProfileSummaryRejectsMovesFromWithdrawn()
    {
        var item = SkillProfileSummary.NewSkillProfileSummary("code-994", "name-547");
        item.Status = SkillProfileSummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SkillProfileSummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SkillProfileSummaryStartsInProposed();
        SkillProfileSummaryMovesToApproved();
        SkillProfileSummaryRejectsMovesFromWithdrawn();
    }
}
