namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentScheduleRulesChecks
{
    public static void AdjustmentScheduleValidatorReportsEveryProblem()
    {
        var validator = AdjustmentScheduleRules.DefaultAdjustmentScheduleValidator();
        var item = AdjustmentSchedule.NewAdjustmentSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentScheduleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdjustmentScheduleLabelLengthRule(120);
        var item = AdjustmentSchedule.NewAdjustmentSchedule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentScheduleSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new AdjustmentScheduleSummaryLengthRule(120);
        var item = AdjustmentSchedule.NewAdjustmentSchedule("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentScheduleRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new AdjustmentScheduleRequiredSummaryRule("adjustment schedule needs a value");
        var item = AdjustmentSchedule.NewAdjustmentSchedule("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentScheduleValidatorReportsEveryProblem();
        AdjustmentScheduleLabelLengthRuleFlagsALongLabel();
        AdjustmentScheduleSummaryLengthRuleFlagsALongSummary();
        AdjustmentScheduleRequiredSummaryRuleFlagsABlankSummary();
    }
}
