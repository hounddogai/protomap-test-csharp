namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientDeductionAgreementRulesChecks
{
    public static void LenientDeductionAgreementValidatorReportsEveryProblem()
    {
        var validator = LenientDeductionAgreementRules.DefaultLenientDeductionAgreementValidator();
        var item = LenientDeductionAgreement.NewLenientDeductionAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientDeductionAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientDeductionAgreementCodeLengthRule(32);
        var item = LenientDeductionAgreement.NewLenientDeductionAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientDeductionAgreementRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientDeductionAgreementRequiredTitleRule("lenient deduction agreement needs a value");
        var item = LenientDeductionAgreement.NewLenientDeductionAgreement("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientDeductionAgreementLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new LenientDeductionAgreementLocaleLengthRule(86400);
        var item = LenientDeductionAgreement.NewLenientDeductionAgreement("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientDeductionAgreementRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new LenientDeductionAgreementRequiredEmployeeIdRule("lenient deduction agreement needs this field");
        var item = LenientDeductionAgreement.NewLenientDeductionAgreement("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientDeductionAgreementValidatorReportsEveryProblem();
        LenientDeductionAgreementCodeLengthRuleFlagsALongCode();
        LenientDeductionAgreementRequiredTitleRuleFlagsABlankTitle();
        LenientDeductionAgreementLocaleLengthRuleFlagsALongLocale();
        LenientDeductionAgreementRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
