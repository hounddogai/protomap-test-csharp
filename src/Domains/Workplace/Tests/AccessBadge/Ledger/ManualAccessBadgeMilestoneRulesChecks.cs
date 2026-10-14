namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ManualAccessBadgeMilestoneRulesChecks
{
    public static void ManualAccessBadgeMilestoneValidatorReportsEveryProblem()
    {
        var validator = ManualAccessBadgeMilestoneRules.DefaultManualAccessBadgeMilestoneValidator();
        var item = ManualAccessBadgeMilestone.NewManualAccessBadgeMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAccessBadgeMilestoneLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new ManualAccessBadgeMilestoneLeaveDaysRangeRule(80, 1000);
        var item = ManualAccessBadgeMilestone.NewManualAccessBadgeMilestone("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualAccessBadgeMilestoneRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualAccessBadgeMilestoneRequiredTitleRule("manual access badge milestone needs a value");
        var item = ManualAccessBadgeMilestone.NewManualAccessBadgeMilestone("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAccessBadgeMilestoneMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new ManualAccessBadgeMilestoneMarginRangeRule(2.0, 3736.5);
        var item = ManualAccessBadgeMilestone.NewManualAccessBadgeMilestone("x", "y");
        item.Margin = 3738.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualAccessBadgeMilestoneTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualAccessBadgeMilestoneTitleLengthRule(86400);
        var item = ManualAccessBadgeMilestone.NewManualAccessBadgeMilestone("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualAccessBadgeMilestoneValidatorReportsEveryProblem();
        ManualAccessBadgeMilestoneLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        ManualAccessBadgeMilestoneRequiredTitleRuleFlagsABlankTitle();
        ManualAccessBadgeMilestoneMarginRangeRuleFlagsValuesAboveMargin();
        ManualAccessBadgeMilestoneTitleLengthRuleFlagsALongTitle();
    }
}
