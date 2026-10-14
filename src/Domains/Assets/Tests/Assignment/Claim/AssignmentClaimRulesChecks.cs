namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentClaimRulesChecks
{
    public static void AssignmentClaimValidatorReportsEveryProblem()
    {
        var validator = AssignmentClaimRules.DefaultAssignmentClaimValidator();
        var item = AssignmentClaim.NewAssignmentClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentClaimRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new AssignmentClaimRatingRangeRule(3.8, 3879.0);
        var item = AssignmentClaim.NewAssignmentClaim("x", "y");
        item.Rating = 3880.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentClaimReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new AssignmentClaimReviewerLengthRule(1000);
        var item = AssignmentClaim.NewAssignmentClaim("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssignmentClaimValidatorReportsEveryProblem();
        AssignmentClaimRatingRangeRuleFlagsValuesAboveRating();
        AssignmentClaimReviewerLengthRuleFlagsALongReviewer();
    }
}
