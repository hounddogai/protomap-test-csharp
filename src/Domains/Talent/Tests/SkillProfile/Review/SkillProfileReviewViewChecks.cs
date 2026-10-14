namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class SkillProfileReviewViewChecks
{
    public static void SkillProfileReviewViewShowsTheStatusInCapitals()
    {
        var item = SkillProfileReview.NewSkillProfileReview("code-232", "name-200");
        var view = SkillProfileReviewView.ToSkillProfileReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SkillProfileReviewView.ToSkillProfileReviewViews(new List<SkillProfileReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SkillProfileReviewViewShowsTheStatusInCapitals();
    }
}
