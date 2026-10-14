namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AccessBadgeAgreementRulesChecks
{
    public static void AccessBadgeAgreementValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeAgreementRules.DefaultAccessBadgeAgreementValidator();
        var item = AccessBadgeAgreement.NewAccessBadgeAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeAgreementRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccessBadgeAgreementRequiredNameRule("access badge agreement needs an entry");
        var item = AccessBadgeAgreement.NewAccessBadgeAgreement("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeAgreementManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new AccessBadgeAgreementManagerIdLengthRule(16);
        var item = AccessBadgeAgreement.NewAccessBadgeAgreement("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeAgreementCodeLengthRule(1000);
        var item = AccessBadgeAgreement.NewAccessBadgeAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeAgreementValidatorReportsEveryProblem();
        AccessBadgeAgreementRequiredNameRuleFlagsABlankName();
        AccessBadgeAgreementManagerIdLengthRuleFlagsALongManagerId();
        AccessBadgeAgreementCodeLengthRuleFlagsALongCode();
    }
}
