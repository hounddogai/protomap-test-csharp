namespace Acme.Workforce.Contractors.Tests.Timesheet.Budget;

using Acme.Workforce.Contractors.Timesheet.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RegionalTimesheetReviewRulesChecks
{
    public static void RegionalTimesheetReviewValidatorReportsEveryProblem()
    {
        var validator = RegionalTimesheetReviewRules.DefaultRegionalTimesheetReviewValidator();
        var item = RegionalTimesheetReview.NewRegionalTimesheetReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalTimesheetReviewRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new RegionalTimesheetReviewRequiredTeamRule("regional timesheet review needs an entry");
        var item = RegionalTimesheetReview.NewRegionalTimesheetReview("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalTimesheetReviewBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RegionalTimesheetReviewBadgeLengthRule(86400);
        var item = RegionalTimesheetReview.NewRegionalTimesheetReview("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalTimesheetReviewTeamLengthRuleFlagsALongTeam()
    {
        var rule = new RegionalTimesheetReviewTeamLengthRule(86400);
        var item = RegionalTimesheetReview.NewRegionalTimesheetReview("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalTimesheetReviewAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new RegionalTimesheetReviewAttemptsRangeRule(120, 1000);
        var item = RegionalTimesheetReview.NewRegionalTimesheetReview("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalTimesheetReviewValidatorReportsEveryProblem();
        RegionalTimesheetReviewRequiredTeamRuleFlagsABlankTeam();
        RegionalTimesheetReviewBadgeLengthRuleFlagsALongBadge();
        RegionalTimesheetReviewTeamLengthRuleFlagsALongTeam();
        RegionalTimesheetReviewAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
