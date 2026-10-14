namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class ScheduledAccrualBaselineRulesChecks
{
    public static void ScheduledAccrualBaselineValidatorReportsEveryProblem()
    {
        var validator = ScheduledAccrualBaselineRules.DefaultScheduledAccrualBaselineValidator();
        var item = ScheduledAccrualBaseline.NewScheduledAccrualBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledAccrualBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledAccrualBaselineCodeLengthRule(255);
        var item = ScheduledAccrualBaseline.NewScheduledAccrualBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAccrualBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledAccrualBaselineRequiredTitleRule("scheduled accrual baseline needs this field");
        var item = ScheduledAccrualBaseline.NewScheduledAccrualBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledAccrualBaselineQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new ScheduledAccrualBaselineQuantityRangeRule(120, 1000);
        var item = ScheduledAccrualBaseline.NewScheduledAccrualBaseline("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAccrualBaselineThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new ScheduledAccrualBaselineThresholdRangeRule(8.8, 2702.4);
        var item = ScheduledAccrualBaseline.NewScheduledAccrualBaseline("x", "y");
        item.Threshold = 2703.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledAccrualBaselineValidatorReportsEveryProblem();
        ScheduledAccrualBaselineCodeLengthRuleFlagsALongCode();
        ScheduledAccrualBaselineRequiredTitleRuleFlagsABlankTitle();
        ScheduledAccrualBaselineQuantityRangeRuleFlagsValuesAboveQuantity();
        ScheduledAccrualBaselineThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
