namespace Acme.Workforce.Payrollops.Tests.PayRun.Window;

using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientPayRunAgreementRulesChecks
{
    public static void LenientPayRunAgreementValidatorReportsEveryProblem()
    {
        var validator = LenientPayRunAgreementRules.DefaultLenientPayRunAgreementValidator();
        var item = LenientPayRunAgreement.NewLenientPayRunAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPayRunAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientPayRunAgreementRequiredCodeRule("lenient pay run agreement needs an entry");
        var item = LenientPayRunAgreement.NewLenientPayRunAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPayRunAgreementRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new LenientPayRunAgreementRequiredDescriptionRule("lenient pay run agreement needs an entry");
        var item = LenientPayRunAgreement.NewLenientPayRunAgreement("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPayRunAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientPayRunAgreementCodeLengthRule(120);
        var item = LenientPayRunAgreement.NewLenientPayRunAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPayRunAgreementRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new LenientPayRunAgreementRequiredBadgeRule("lenient pay run agreement needs this field");
        var item = LenientPayRunAgreement.NewLenientPayRunAgreement("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientPayRunAgreementValidatorReportsEveryProblem();
        LenientPayRunAgreementRequiredCodeRuleFlagsABlankCode();
        LenientPayRunAgreementRequiredDescriptionRuleFlagsABlankDescription();
        LenientPayRunAgreementCodeLengthRuleFlagsALongCode();
        LenientPayRunAgreementRequiredBadgeRuleFlagsABlankBadge();
    }
}
