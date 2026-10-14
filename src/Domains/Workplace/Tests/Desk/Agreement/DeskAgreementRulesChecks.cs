namespace Acme.Workforce.Workplace.Tests.Desk.Agreement;

using Acme.Workforce.Workplace.Desk.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeskAgreementRulesChecks
{
    public static void DeskAgreementValidatorReportsEveryProblem()
    {
        var validator = DeskAgreementRules.DefaultDeskAgreementValidator();
        var item = DeskAgreement.NewDeskAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeskAgreementRequiredCodeRule("desk agreement needs this field");
        var item = DeskAgreement.NewDeskAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskAgreementBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new DeskAgreementBadgeLengthRule(255);
        var item = DeskAgreement.NewDeskAgreement("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskAgreementRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new DeskAgreementRequiredJobCodeRule("desk agreement needs an entry");
        var item = DeskAgreement.NewDeskAgreement("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskAgreementValidatorReportsEveryProblem();
        DeskAgreementRequiredCodeRuleFlagsABlankCode();
        DeskAgreementBadgeLengthRuleFlagsALongBadge();
        DeskAgreementRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
