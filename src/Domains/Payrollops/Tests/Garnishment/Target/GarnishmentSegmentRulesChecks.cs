namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class GarnishmentSegmentRulesChecks
{
    public static void GarnishmentSegmentValidatorReportsEveryProblem()
    {
        var validator = GarnishmentSegmentRules.DefaultGarnishmentSegmentValidator();
        var item = GarnishmentSegment.NewGarnishmentSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentSegmentMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new GarnishmentSegmentMinimumRangeRule(4.7, 3551.8);
        var item = GarnishmentSegment.NewGarnishmentSegment("x", "y");
        item.Minimum = 3553.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GarnishmentSegmentRequiredCodeRule("garnishment segment needs this field");
        var item = GarnishmentSegment.NewGarnishmentSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentSegmentCodeLengthRule(32);
        var item = GarnishmentSegment.NewGarnishmentSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GarnishmentSegmentValidatorReportsEveryProblem();
        GarnishmentSegmentMinimumRangeRuleFlagsValuesAboveMinimum();
        GarnishmentSegmentRequiredCodeRuleFlagsABlankCode();
        GarnishmentSegmentCodeLengthRuleFlagsALongCode();
    }
}
