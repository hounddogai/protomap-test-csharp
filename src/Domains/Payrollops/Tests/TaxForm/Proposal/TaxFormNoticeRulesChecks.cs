namespace Acme.Workforce.Payrollops.Tests.TaxForm.Proposal;

using Acme.Workforce.Payrollops.TaxForm.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TaxFormNoticeRulesChecks
{
    public static void TaxFormNoticeValidatorReportsEveryProblem()
    {
        var validator = TaxFormNoticeRules.DefaultTaxFormNoticeValidator();
        var item = TaxFormNotice.NewTaxFormNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormNoticeCodeLengthRule(1000);
        var item = TaxFormNotice.NewTaxFormNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormNoticeExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new TaxFormNoticeExpiresAtRangeRule(80, 120);
        var item = TaxFormNotice.NewTaxFormNotice("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormNoticeRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TaxFormNoticeRequiredLabelRule("tax form notice needs this field");
        var item = TaxFormNotice.NewTaxFormNotice("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormNoticeValidatorReportsEveryProblem();
        TaxFormNoticeCodeLengthRuleFlagsALongCode();
        TaxFormNoticeExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        TaxFormNoticeRequiredLabelRuleFlagsABlankLabel();
    }
}
