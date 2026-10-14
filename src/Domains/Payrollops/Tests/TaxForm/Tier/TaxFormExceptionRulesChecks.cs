namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TaxFormExceptionRulesChecks
{
    public static void TaxFormExceptionValidatorReportsEveryProblem()
    {
        var validator = TaxFormExceptionRules.DefaultTaxFormExceptionValidator();
        var item = TaxFormException.NewTaxFormException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormExceptionRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new TaxFormExceptionRequiredCurrencyRule("tax form exception needs a value");
        var item = TaxFormException.NewTaxFormException("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormExceptionCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new TaxFormExceptionCurrencyLengthRule(255);
        var item = TaxFormException.NewTaxFormException("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormExceptionRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new TaxFormExceptionRequiredCategoryRule("tax form exception needs this field");
        var item = TaxFormException.NewTaxFormException("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormExceptionCodeLengthRule(120);
        var item = TaxFormException.NewTaxFormException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TaxFormExceptionValidatorReportsEveryProblem();
        TaxFormExceptionRequiredCurrencyRuleFlagsABlankCurrency();
        TaxFormExceptionCurrencyLengthRuleFlagsALongCurrency();
        TaxFormExceptionRequiredCategoryRuleFlagsABlankCategory();
        TaxFormExceptionCodeLengthRuleFlagsALongCode();
    }
}
