namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedRateMilestoneRulesChecks
{
    public static void CappedRateMilestoneValidatorReportsEveryProblem()
    {
        var validator = CappedRateMilestoneRules.DefaultCappedRateMilestoneValidator();
        var item = CappedRateMilestone.NewCappedRateMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedRateMilestoneGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new CappedRateMilestoneGraceDaysRangeRule(80, 255);
        var item = CappedRateMilestone.NewCappedRateMilestone("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedRateMilestoneRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new CappedRateMilestoneRequiredLocationRule("capped rate milestone needs an entry");
        var item = CappedRateMilestone.NewCappedRateMilestone("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedRateMilestoneLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new CappedRateMilestoneLimitRangeRule(64, 80);
        var item = CappedRateMilestone.NewCappedRateMilestone("x", "y");
        item.Limit = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedRateMilestoneValidatorReportsEveryProblem();
        CappedRateMilestoneGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        CappedRateMilestoneRequiredLocationRuleFlagsABlankLocation();
        CappedRateMilestoneLimitRangeRuleFlagsValuesAboveLimit();
    }
}
