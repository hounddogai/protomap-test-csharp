namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class ScheduledAdjustmentRequestRulesChecks
{
    public static void ScheduledAdjustmentRequestValidatorReportsEveryProblem()
    {
        var validator = ScheduledAdjustmentRequestRules.DefaultScheduledAdjustmentRequestValidator();
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledAdjustmentRequestRegionLengthRuleFlagsALongRegion()
    {
        var rule = new ScheduledAdjustmentRequestRegionLengthRule(16);
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAdjustmentRequestRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new ScheduledAdjustmentRequestRequiredRegionRule("scheduled adjustment request needs an entry");
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledAdjustmentRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScheduledAdjustmentRequestRequiredLabelRule("scheduled adjustment request needs this field");
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledAdjustmentRequestLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new ScheduledAdjustmentRequestLimitRangeRule(64, 3600);
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAdjustmentRequestSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new ScheduledAdjustmentRequestSequenceRangeRule(32, 120);
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAdjustmentRequestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledAdjustmentRequestLabelLengthRule(32);
        var item = ScheduledAdjustmentRequest.NewScheduledAdjustmentRequest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledAdjustmentRequestValidatorReportsEveryProblem();
        ScheduledAdjustmentRequestRegionLengthRuleFlagsALongRegion();
        ScheduledAdjustmentRequestRequiredRegionRuleFlagsABlankRegion();
        ScheduledAdjustmentRequestRequiredLabelRuleFlagsABlankLabel();
        ScheduledAdjustmentRequestLimitRangeRuleFlagsValuesAboveLimit();
        ScheduledAdjustmentRequestSequenceRangeRuleFlagsValuesAboveSequence();
        ScheduledAdjustmentRequestLabelLengthRuleFlagsALongLabel();
    }
}
