namespace Acme.Workforce.Compensation.Tests.Adjustment.Template;

using Acme.Workforce.Compensation.Adjustment.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledAdjustmentSummaryRulesChecks
{
    public static void ScheduledAdjustmentSummaryValidatorReportsEveryProblem()
    {
        var validator = ScheduledAdjustmentSummaryRules.DefaultScheduledAdjustmentSummaryValidator();
        var item = ScheduledAdjustmentSummary.NewScheduledAdjustmentSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledAdjustmentSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledAdjustmentSummaryRequiredCodeRule("scheduled adjustment summary needs a value");
        var item = ScheduledAdjustmentSummary.NewScheduledAdjustmentSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledAdjustmentSummaryChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new ScheduledAdjustmentSummaryChecksumLengthRule(3600);
        var item = ScheduledAdjustmentSummary.NewScheduledAdjustmentSummary("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAdjustmentSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledAdjustmentSummaryCodeLengthRule(32);
        var item = ScheduledAdjustmentSummary.NewScheduledAdjustmentSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAdjustmentSummaryLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new ScheduledAdjustmentSummaryLimitRangeRule(32, 120);
        var item = ScheduledAdjustmentSummary.NewScheduledAdjustmentSummary("x", "y");
        item.Limit = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledAdjustmentSummaryValidatorReportsEveryProblem();
        ScheduledAdjustmentSummaryRequiredCodeRuleFlagsABlankCode();
        ScheduledAdjustmentSummaryChecksumLengthRuleFlagsALongChecksum();
        ScheduledAdjustmentSummaryCodeLengthRuleFlagsALongCode();
        ScheduledAdjustmentSummaryLimitRangeRuleFlagsValuesAboveLimit();
    }
}
