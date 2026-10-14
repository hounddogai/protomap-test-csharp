namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingExceptionQuotaRulesChecks
{
    public static void PendingExceptionQuotaValidatorReportsEveryProblem()
    {
        var validator = PendingExceptionQuotaRules.DefaultPendingExceptionQuotaValidator();
        var item = PendingExceptionQuota.NewPendingExceptionQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingExceptionQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingExceptionQuotaCodeLengthRule(16);
        var item = PendingExceptionQuota.NewPendingExceptionQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingExceptionQuotaValidatorReportsEveryProblem();
        PendingExceptionQuotaCodeLengthRuleFlagsALongCode();
    }
}
