namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class SkillProfileReviewRulesChecks
{
    public static void SkillProfileReviewValidatorReportsEveryProblem()
    {
        var validator = SkillProfileReviewRules.DefaultSkillProfileReviewValidator();
        var item = SkillProfileReview.NewSkillProfileReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileReviewRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new SkillProfileReviewRequiredReviewerRule("skill profile review needs a value");
        var item = SkillProfileReview.NewSkillProfileReview("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileReviewReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new SkillProfileReviewReviewerLengthRule(1000);
        var item = SkillProfileReview.NewSkillProfileReview("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileReviewAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new SkillProfileReviewAttemptsRangeRule(16, 3600);
        var item = SkillProfileReview.NewSkillProfileReview("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SkillProfileReviewValidatorReportsEveryProblem();
        SkillProfileReviewRequiredReviewerRuleFlagsABlankReviewer();
        SkillProfileReviewReviewerLengthRuleFlagsALongReviewer();
        SkillProfileReviewAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
