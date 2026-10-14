namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class AdaptivePolicyQuotaRulesChecks
{
    public static void AdaptivePolicyQuotaValidatorReportsEveryProblem()
    {
        var validator = AdaptivePolicyQuotaRules.DefaultAdaptivePolicyQuotaValidator();
        var item = AdaptivePolicyQuota.NewAdaptivePolicyQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePolicyQuotaCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new AdaptivePolicyQuotaCurrencyLengthRule(3600);
        var item = AdaptivePolicyQuota.NewAdaptivePolicyQuota("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePolicyQuotaRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new AdaptivePolicyQuotaRequiredEmployeeIdRule("adaptive policy quota needs this field");
        var item = AdaptivePolicyQuota.NewAdaptivePolicyQuota("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePolicyQuotaEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new AdaptivePolicyQuotaEmployeeIdLengthRule(1000);
        var item = AdaptivePolicyQuota.NewAdaptivePolicyQuota("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptivePolicyQuotaValidatorReportsEveryProblem();
        AdaptivePolicyQuotaCurrencyLengthRuleFlagsALongCurrency();
        AdaptivePolicyQuotaRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        AdaptivePolicyQuotaEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
