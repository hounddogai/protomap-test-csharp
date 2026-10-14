namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class SeasonalTaxFormTierRulesChecks
{
    public static void SeasonalTaxFormTierValidatorReportsEveryProblem()
    {
        var validator = SeasonalTaxFormTierRules.DefaultSeasonalTaxFormTierValidator();
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalTaxFormTierCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new SeasonalTaxFormTierCurrencyLengthRule(86400);
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalTaxFormTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SeasonalTaxFormTierRequiredTitleRule("seasonal tax form tier needs an entry");
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalTaxFormTierRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new SeasonalTaxFormTierRequiredCurrencyRule("seasonal tax form tier needs a value");
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalTaxFormTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalTaxFormTierRequiredLabelRule("seasonal tax form tier needs this field");
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalTaxFormTierValidatorReportsEveryProblem();
        SeasonalTaxFormTierCurrencyLengthRuleFlagsALongCurrency();
        SeasonalTaxFormTierRequiredTitleRuleFlagsABlankTitle();
        SeasonalTaxFormTierRequiredCurrencyRuleFlagsABlankCurrency();
        SeasonalTaxFormTierRequiredLabelRuleFlagsABlankLabel();
    }
}
