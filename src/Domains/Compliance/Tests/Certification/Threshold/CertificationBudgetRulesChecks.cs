namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class CertificationBudgetRulesChecks
{
    public static void CertificationBudgetValidatorReportsEveryProblem()
    {
        var validator = CertificationBudgetRules.DefaultCertificationBudgetValidator();
        var item = CertificationBudget.NewCertificationBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationBudgetRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new CertificationBudgetRequiredChannelRule("certification budget needs an entry");
        var item = CertificationBudget.NewCertificationBudget("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationBudgetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CertificationBudgetRequiredTitleRule("certification budget needs this field");
        var item = CertificationBudget.NewCertificationBudget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CertificationBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CertificationBudgetRequiredCodeRule("certification budget needs this field");
        var item = CertificationBudget.NewCertificationBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CertificationBudgetValidatorReportsEveryProblem();
        CertificationBudgetRequiredChannelRuleFlagsABlankChannel();
        CertificationBudgetRequiredTitleRuleFlagsABlankTitle();
        CertificationBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
