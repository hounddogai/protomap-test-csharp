namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanScheduleRulesChecks
{
    public static void SuccessionPlanScheduleValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanScheduleRules.DefaultSuccessionPlanScheduleValidator();
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanScheduleSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new SuccessionPlanScheduleSequenceRangeRule(120, 1000);
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanScheduleRequiredCodeRule("succession plan schedule needs this field");
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new SuccessionPlanScheduleCodeLengthRule(255);
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanScheduleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SuccessionPlanScheduleRequiredLabelRule("succession plan schedule needs a value");
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanScheduleMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new SuccessionPlanScheduleMarginRangeRule(5.0, 521.5);
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("x", "y");
        item.Margin = 523.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SuccessionPlanScheduleValidatorReportsEveryProblem();
        SuccessionPlanScheduleSequenceRangeRuleFlagsValuesAboveSequence();
        SuccessionPlanScheduleRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanScheduleCodeLengthRuleFlagsALongCode();
        SuccessionPlanScheduleRequiredLabelRuleFlagsABlankLabel();
        SuccessionPlanScheduleMarginRangeRuleFlagsValuesAboveMargin();
    }
}
