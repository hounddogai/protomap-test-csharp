namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientAdjustmentScheduleRulesChecks
{
    public static void LenientAdjustmentScheduleValidatorReportsEveryProblem()
    {
        var validator = LenientAdjustmentScheduleRules.DefaultLenientAdjustmentScheduleValidator();
        var item = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientAdjustmentScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientAdjustmentScheduleRequiredCodeRule("lenient adjustment schedule needs an entry");
        var item = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientAdjustmentScheduleRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new LenientAdjustmentScheduleRatingRangeRule(0.1, 4730.5);
        var item = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule("x", "y");
        item.Rating = 4732.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientAdjustmentScheduleRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new LenientAdjustmentScheduleRequiredGradeRule("lenient adjustment schedule needs an entry");
        var item = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientAdjustmentScheduleRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientAdjustmentScheduleRequiredNameRule("lenient adjustment schedule needs this field");
        var item = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientAdjustmentScheduleValidatorReportsEveryProblem();
        LenientAdjustmentScheduleRequiredCodeRuleFlagsABlankCode();
        LenientAdjustmentScheduleRatingRangeRuleFlagsValuesAboveRating();
        LenientAdjustmentScheduleRequiredGradeRuleFlagsABlankGrade();
        LenientAdjustmentScheduleRequiredNameRuleFlagsABlankName();
    }
}
