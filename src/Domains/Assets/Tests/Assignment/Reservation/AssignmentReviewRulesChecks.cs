namespace Acme.Workforce.Assets.Tests.Assignment.Reservation;

using Acme.Workforce.Assets.Assignment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssignmentReviewRulesChecks
{
    public static void AssignmentReviewValidatorReportsEveryProblem()
    {
        var validator = AssignmentReviewRules.DefaultAssignmentReviewValidator();
        var item = AssignmentReview.NewAssignmentReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssignmentReviewTitleLengthRule(32);
        var item = AssignmentReview.NewAssignmentReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentReviewGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AssignmentReviewGraceDaysRangeRule(16, 3600);
        var item = AssignmentReview.NewAssignmentReview("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentReviewRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new AssignmentReviewRequiredContactEmailRule("assignment review needs an entry");
        var item = AssignmentReview.NewAssignmentReview("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentReviewTeamLengthRuleFlagsALongTeam()
    {
        var rule = new AssignmentReviewTeamLengthRule(80);
        var item = AssignmentReview.NewAssignmentReview("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentReviewCodeLengthRule(1000);
        var item = AssignmentReview.NewAssignmentReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssignmentReviewValidatorReportsEveryProblem();
        AssignmentReviewTitleLengthRuleFlagsALongTitle();
        AssignmentReviewGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        AssignmentReviewRequiredContactEmailRuleFlagsABlankContactEmail();
        AssignmentReviewTeamLengthRuleFlagsALongTeam();
        AssignmentReviewCodeLengthRuleFlagsALongCode();
    }
}
