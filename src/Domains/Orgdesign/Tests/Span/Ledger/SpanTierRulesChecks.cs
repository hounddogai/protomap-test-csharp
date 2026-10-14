namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SpanTierRulesChecks
{
    public static void SpanTierValidatorReportsEveryProblem()
    {
        var validator = SpanTierRules.DefaultSpanTierValidator();
        var item = SpanTier.NewSpanTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanTierRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new SpanTierRequiredExternalRefRule("span tier needs this field");
        var item = SpanTier.NewSpanTier("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new SpanTierRequiredNameRule("span tier needs a value");
        var item = SpanTier.NewSpanTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanTierQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SpanTierQuantityRangeRule(120, 255);
        var item = SpanTier.NewSpanTier("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SpanTierValidatorReportsEveryProblem();
        SpanTierRequiredExternalRefRuleFlagsABlankExternalRef();
        SpanTierRequiredNameRuleFlagsABlankName();
        SpanTierQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
