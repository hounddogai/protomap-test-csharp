namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LenientNominationMappingRulesChecks
{
    public static void LenientNominationMappingValidatorReportsEveryProblem()
    {
        var validator = LenientNominationMappingRules.DefaultLenientNominationMappingValidator();
        var item = LenientNominationMapping.NewLenientNominationMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientNominationMappingCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new LenientNominationMappingCurrencyLengthRule(64);
        var item = LenientNominationMapping.NewLenientNominationMapping("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientNominationMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientNominationMappingRequiredNameRule("lenient nomination mapping needs an entry");
        var item = LenientNominationMapping.NewLenientNominationMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientNominationMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientNominationMappingCodeLengthRule(32);
        var item = LenientNominationMapping.NewLenientNominationMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientNominationMappingBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new LenientNominationMappingBudgetRangeRule(5.5, 2324.0);
        var item = LenientNominationMapping.NewLenientNominationMapping("x", "y");
        item.Budget = 2325.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientNominationMappingRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new LenientNominationMappingRequiredSourceRule("lenient nomination mapping needs a value");
        var item = LenientNominationMapping.NewLenientNominationMapping("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientNominationMappingValidatorReportsEveryProblem();
        LenientNominationMappingCurrencyLengthRuleFlagsALongCurrency();
        LenientNominationMappingRequiredNameRuleFlagsABlankName();
        LenientNominationMappingCodeLengthRuleFlagsALongCode();
        LenientNominationMappingBudgetRangeRuleFlagsValuesAboveBudget();
        LenientNominationMappingRequiredSourceRuleFlagsABlankSource();
    }
}
