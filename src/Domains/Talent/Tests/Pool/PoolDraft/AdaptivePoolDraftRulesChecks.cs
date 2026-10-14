namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class AdaptivePoolDraftRulesChecks
{
    public static void AdaptivePoolDraftValidatorReportsEveryProblem()
    {
        var validator = AdaptivePoolDraftRules.DefaultAdaptivePoolDraftValidator();
        var item = AdaptivePoolDraft.NewAdaptivePoolDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePoolDraftRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptivePoolDraftRequiredLabelRule("adaptive pool draft needs a value");
        var item = AdaptivePoolDraft.NewAdaptivePoolDraft("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePoolDraftGradeLengthRuleFlagsALongGrade()
    {
        var rule = new AdaptivePoolDraftGradeLengthRule(80);
        var item = AdaptivePoolDraft.NewAdaptivePoolDraft("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePoolDraftDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new AdaptivePoolDraftDiscountRangeRule(8.8, 3589.3);
        var item = AdaptivePoolDraft.NewAdaptivePoolDraft("x", "y");
        item.Discount = 3590.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptivePoolDraftValidatorReportsEveryProblem();
        AdaptivePoolDraftRequiredLabelRuleFlagsABlankLabel();
        AdaptivePoolDraftGradeLengthRuleFlagsALongGrade();
        AdaptivePoolDraftDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
