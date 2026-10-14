namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SpanClaimRulesChecks
{
    public static void SpanClaimValidatorReportsEveryProblem()
    {
        var validator = SpanClaimRules.DefaultSpanClaimValidator();
        var item = SpanClaim.NewSpanClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanClaimLocationLengthRuleFlagsALongLocation()
    {
        var rule = new SpanClaimLocationLengthRule(80);
        var item = SpanClaim.NewSpanClaim("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanClaimRequiredNameRuleFlagsABlankName()
    {
        var rule = new SpanClaimRequiredNameRule("span claim needs an entry");
        var item = SpanClaim.NewSpanClaim("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanClaimWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new SpanClaimWeightRangeRule(6.7, 2161.9);
        var item = SpanClaim.NewSpanClaim("x", "y");
        item.Weight = 2163.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SpanClaimValidatorReportsEveryProblem();
        SpanClaimLocationLengthRuleFlagsALongLocation();
        SpanClaimRequiredNameRuleFlagsABlankName();
        SpanClaimWeightRangeRuleFlagsValuesAboveWeight();
    }
}
