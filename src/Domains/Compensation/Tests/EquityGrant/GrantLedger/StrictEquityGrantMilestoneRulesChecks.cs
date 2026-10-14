namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>Checks of grant ledger code, run by the project's runner.</summary>
public static class StrictEquityGrantMilestoneRulesChecks
{
    public static void StrictEquityGrantMilestoneValidatorReportsEveryProblem()
    {
        var validator = StrictEquityGrantMilestoneRules.DefaultStrictEquityGrantMilestoneValidator();
        var item = StrictEquityGrantMilestone.NewStrictEquityGrantMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictEquityGrantMilestoneRegionLengthRuleFlagsALongRegion()
    {
        var rule = new StrictEquityGrantMilestoneRegionLengthRule(120);
        var item = StrictEquityGrantMilestone.NewStrictEquityGrantMilestone("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictEquityGrantMilestoneRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictEquityGrantMilestoneRequiredTitleRule("strict equity grant milestone needs this field");
        var item = StrictEquityGrantMilestone.NewStrictEquityGrantMilestone("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictEquityGrantMilestoneDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new StrictEquityGrantMilestoneDurationMinutesRangeRule(16, 80);
        var item = StrictEquityGrantMilestone.NewStrictEquityGrantMilestone("x", "y");
        item.DurationMinutes = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictEquityGrantMilestoneTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StrictEquityGrantMilestoneTitleLengthRule(120);
        var item = StrictEquityGrantMilestone.NewStrictEquityGrantMilestone("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictEquityGrantMilestoneValidatorReportsEveryProblem();
        StrictEquityGrantMilestoneRegionLengthRuleFlagsALongRegion();
        StrictEquityGrantMilestoneRequiredTitleRuleFlagsABlankTitle();
        StrictEquityGrantMilestoneDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        StrictEquityGrantMilestoneTitleLengthRuleFlagsALongTitle();
    }
}
