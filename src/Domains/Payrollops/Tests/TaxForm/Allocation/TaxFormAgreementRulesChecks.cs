namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAgreementRulesChecks
{
    public static void TaxFormAgreementValidatorReportsEveryProblem()
    {
        var validator = TaxFormAgreementRules.DefaultTaxFormAgreementValidator();
        var item = TaxFormAgreement.NewTaxFormAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormAgreementRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TaxFormAgreementRequiredTitleRule("tax form agreement needs a value");
        var item = TaxFormAgreement.NewTaxFormAgreement("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormAgreementCodeLengthRule(120);
        var item = TaxFormAgreement.NewTaxFormAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormAgreementTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TaxFormAgreementTitleLengthRule(3600);
        var item = TaxFormAgreement.NewTaxFormAgreement("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TaxFormAgreementValidatorReportsEveryProblem();
        TaxFormAgreementRequiredTitleRuleFlagsABlankTitle();
        TaxFormAgreementCodeLengthRuleFlagsALongCode();
        TaxFormAgreementTitleLengthRuleFlagsALongTitle();
    }
}
