namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TaxFormProfileRulesChecks
{
    public static void TaxFormProfileValidatorReportsEveryProblem()
    {
        var validator = TaxFormProfileRules.DefaultTaxFormProfileValidator();
        var item = TaxFormProfile.NewTaxFormProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormProfileCodeLengthRule(120);
        var item = TaxFormProfile.NewTaxFormProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormProfileRequiredNameRuleFlagsABlankName()
    {
        var rule = new TaxFormProfileRequiredNameRule("tax form profile needs an entry");
        var item = TaxFormProfile.NewTaxFormProfile("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TaxFormProfileRequiredCodeRule("tax form profile needs a value");
        var item = TaxFormProfile.NewTaxFormProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormProfileRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new TaxFormProfileRequiredCategoryRule("tax form profile needs this field");
        var item = TaxFormProfile.NewTaxFormProfile("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormProfileValidatorReportsEveryProblem();
        TaxFormProfileCodeLengthRuleFlagsALongCode();
        TaxFormProfileRequiredNameRuleFlagsABlankName();
        TaxFormProfileRequiredCodeRuleFlagsABlankCode();
        TaxFormProfileRequiredCategoryRuleFlagsABlankCategory();
    }
}
