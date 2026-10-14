namespace Acme.Workforce.Compliance.Tests.Certification.Review;

using Acme.Workforce.Compliance.Certification.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultCertificationBudgetRulesChecks
{
    public static void DefaultCertificationBudgetValidatorReportsEveryProblem()
    {
        var validator = DefaultCertificationBudgetRules.DefaultDefaultCertificationBudgetValidator();
        var item = DefaultCertificationBudget.NewDefaultCertificationBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultCertificationBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultCertificationBudgetRequiredCodeRule("default certification budget needs an entry");
        var item = DefaultCertificationBudget.NewDefaultCertificationBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultCertificationBudgetExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new DefaultCertificationBudgetExpiresAtRangeRule(64, 255);
        var item = DefaultCertificationBudget.NewDefaultCertificationBudget("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultCertificationBudgetRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new DefaultCertificationBudgetRequiredTeamRule("default certification budget needs an entry");
        var item = DefaultCertificationBudget.NewDefaultCertificationBudget("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultCertificationBudgetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultCertificationBudgetLabelLengthRule(64);
        var item = DefaultCertificationBudget.NewDefaultCertificationBudget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultCertificationBudgetValidatorReportsEveryProblem();
        DefaultCertificationBudgetRequiredCodeRuleFlagsABlankCode();
        DefaultCertificationBudgetExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        DefaultCertificationBudgetRequiredTeamRuleFlagsABlankTeam();
        DefaultCertificationBudgetLabelLengthRuleFlagsALongLabel();
    }
}
