namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class SkillProfileReviewChecks
{
    public static void SkillProfileReviewStartsInOpen()
    {
        var item = SkillProfileReview.NewSkillProfileReview("code-232", "name-200");
        Check.Equal(item.Status, SkillProfileReviewStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileReviewMovesToResolved()
    {
        var item = SkillProfileReview.NewSkillProfileReview("code-232", "name-200");
        try
        {
            item.MoveTo(SkillProfileReviewStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileReviewStatus.Resolved, "status");
    }

    public static void SkillProfileReviewRejectsMovesFromArchived()
    {
        var item = SkillProfileReview.NewSkillProfileReview("code-232", "name-200");
        item.Status = SkillProfileReviewStatus.Archived;
        Check.Fails(() => item.MoveTo(SkillProfileReviewStatus.Open), "move");
    }

    public static void RunAll()
    {
        SkillProfileReviewStartsInOpen();
        SkillProfileReviewMovesToResolved();
        SkillProfileReviewRejectsMovesFromArchived();
    }
}
