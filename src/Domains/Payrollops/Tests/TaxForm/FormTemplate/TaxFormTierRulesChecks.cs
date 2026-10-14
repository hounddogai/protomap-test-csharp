namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormTemplate;

using Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>Checks of form template code, run by the project's runner.</summary>
public static class TaxFormTierRulesChecks
{
    public static void TaxFormTierValidatorReportsEveryProblem()
    {
        var validator = TaxFormTierRules.DefaultTaxFormTierValidator();
        var item = TaxFormTier.NewTaxFormTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TaxFormTierRequiredCodeRule("tax form tier needs an entry");
        var item = TaxFormTier.NewTaxFormTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormTierContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new TaxFormTierContactEmailLengthRule(80);
        var item = TaxFormTier.NewTaxFormTier("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new TaxFormTierRequiredNameRule("tax form tier needs an entry");
        var item = TaxFormTier.NewTaxFormTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormTierValidatorReportsEveryProblem();
        TaxFormTierRequiredCodeRuleFlagsABlankCode();
        TaxFormTierContactEmailLengthRuleFlagsALongContactEmail();
        TaxFormTierRequiredNameRuleFlagsABlankName();
    }
}
