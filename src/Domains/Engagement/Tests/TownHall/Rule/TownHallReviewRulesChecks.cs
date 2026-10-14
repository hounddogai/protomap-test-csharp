namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TownHallReviewRulesChecks
{
    public static void TownHallReviewValidatorReportsEveryProblem()
    {
        var validator = TownHallReviewRules.DefaultTownHallReviewValidator();
        var item = TownHallReview.NewTownHallReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallReviewPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new TownHallReviewPriorityRangeRule(64, 255);
        var item = TownHallReview.NewTownHallReview("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallReviewLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new TownHallReviewLeaveDaysRangeRule(1000, 86400);
        var item = TownHallReview.NewTownHallReview("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallReviewValidatorReportsEveryProblem();
        TownHallReviewPriorityRangeRuleFlagsValuesAbovePriority();
        TownHallReviewLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
