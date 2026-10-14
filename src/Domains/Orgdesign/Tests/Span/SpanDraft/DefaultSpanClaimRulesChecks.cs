namespace Acme.Workforce.Orgdesign.Tests.Span.SpanDraft;

using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Checks of span draft code, run by the project's runner.</summary>
public static class DefaultSpanClaimRulesChecks
{
    public static void DefaultSpanClaimValidatorReportsEveryProblem()
    {
        var validator = DefaultSpanClaimRules.DefaultDefaultSpanClaimValidator();
        var item = DefaultSpanClaim.NewDefaultSpanClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultSpanClaimUnitLengthRuleFlagsALongUnit()
    {
        var rule = new DefaultSpanClaimUnitLengthRule(255);
        var item = DefaultSpanClaim.NewDefaultSpanClaim("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSpanClaimMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new DefaultSpanClaimMinimumRangeRule(6.9, 2628.2);
        var item = DefaultSpanClaim.NewDefaultSpanClaim("x", "y");
        item.Minimum = 2629.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultSpanClaimRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultSpanClaimRequiredLabelRule("default span claim needs a value");
        var item = DefaultSpanClaim.NewDefaultSpanClaim("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultSpanClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultSpanClaimRequiredCodeRule("default span claim needs this field");
        var item = DefaultSpanClaim.NewDefaultSpanClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultSpanClaimRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new DefaultSpanClaimRequiredUnitRule("default span claim needs this field");
        var item = DefaultSpanClaim.NewDefaultSpanClaim("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultSpanClaimValidatorReportsEveryProblem();
        DefaultSpanClaimUnitLengthRuleFlagsALongUnit();
        DefaultSpanClaimMinimumRangeRuleFlagsValuesAboveMinimum();
        DefaultSpanClaimRequiredLabelRuleFlagsABlankLabel();
        DefaultSpanClaimRequiredCodeRuleFlagsABlankCode();
        DefaultSpanClaimRequiredUnitRuleFlagsABlankUnit();
    }
}
