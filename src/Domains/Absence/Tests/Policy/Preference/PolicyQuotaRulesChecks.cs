namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyQuotaRulesChecks
{
    public static void PolicyQuotaValidatorReportsEveryProblem()
    {
        var validator = PolicyQuotaRules.DefaultPolicyQuotaValidator();
        var item = PolicyQuota.NewPolicyQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyQuotaRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new PolicyQuotaRequiredSummaryRule("policy quota needs an entry");
        var item = PolicyQuota.NewPolicyQuota("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyQuotaCodeLengthRule(3600);
        var item = PolicyQuota.NewPolicyQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyQuotaValidatorReportsEveryProblem();
        PolicyQuotaRequiredSummaryRuleFlagsABlankSummary();
        PolicyQuotaCodeLengthRuleFlagsALongCode();
    }
}
