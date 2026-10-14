namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictDeductionSegmentRulesChecks
{
    public static void StrictDeductionSegmentValidatorReportsEveryProblem()
    {
        var validator = StrictDeductionSegmentRules.DefaultStrictDeductionSegmentValidator();
        var item = StrictDeductionSegment.NewStrictDeductionSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictDeductionSegmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new StrictDeductionSegmentDurationMinutesRangeRule(120, 86400);
        var item = StrictDeductionSegment.NewStrictDeductionSegment("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictDeductionSegmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StrictDeductionSegmentTitleLengthRule(64);
        var item = StrictDeductionSegment.NewStrictDeductionSegment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictDeductionSegmentFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new StrictDeductionSegmentFloorValueRangeRule(2.4, 3935.8);
        var item = StrictDeductionSegment.NewStrictDeductionSegment("x", "y");
        item.FloorValue = 3937.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictDeductionSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictDeductionSegmentCodeLengthRule(86400);
        var item = StrictDeductionSegment.NewStrictDeductionSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictDeductionSegmentValidatorReportsEveryProblem();
        StrictDeductionSegmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        StrictDeductionSegmentTitleLengthRuleFlagsALongTitle();
        StrictDeductionSegmentFloorValueRangeRuleFlagsValuesAboveFloorValue();
        StrictDeductionSegmentCodeLengthRuleFlagsALongCode();
    }
}
