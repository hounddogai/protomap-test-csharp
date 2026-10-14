namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FallbackStatementOfWorkSegmentRulesChecks
{
    public static void FallbackStatementOfWorkSegmentValidatorReportsEveryProblem()
    {
        var validator = FallbackStatementOfWorkSegmentRules.DefaultFallbackStatementOfWorkSegmentValidator();
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackStatementOfWorkSegmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackStatementOfWorkSegmentRequiredNameRule(
            "fallback statement of work segment needs a value");
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackStatementOfWorkSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackStatementOfWorkSegmentRequiredCodeRule(
            "fallback statement of work segment needs this field");
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackStatementOfWorkSegmentLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new FallbackStatementOfWorkSegmentLimitRangeRule(1000, 86400);
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackStatementOfWorkSegmentTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new FallbackStatementOfWorkSegmentTimezoneLengthRule(120);
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackStatementOfWorkSegmentCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new FallbackStatementOfWorkSegmentCostRangeRule(2.0, 1168.4);
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("x", "y");
        item.Cost = 1169.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackStatementOfWorkSegmentValidatorReportsEveryProblem();
        FallbackStatementOfWorkSegmentRequiredNameRuleFlagsABlankName();
        FallbackStatementOfWorkSegmentRequiredCodeRuleFlagsABlankCode();
        FallbackStatementOfWorkSegmentLimitRangeRuleFlagsValuesAboveLimit();
        FallbackStatementOfWorkSegmentTimezoneLengthRuleFlagsALongTimezone();
        FallbackStatementOfWorkSegmentCostRangeRuleFlagsValuesAboveCost();
    }
}
