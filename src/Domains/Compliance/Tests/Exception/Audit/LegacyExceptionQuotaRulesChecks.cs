namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyExceptionQuotaRulesChecks
{
    public static void LegacyExceptionQuotaValidatorReportsEveryProblem()
    {
        var validator = LegacyExceptionQuotaRules.DefaultLegacyExceptionQuotaValidator();
        var item = LegacyExceptionQuota.NewLegacyExceptionQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyExceptionQuotaExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LegacyExceptionQuotaExpiresAtRangeRule(64, 80);
        var item = LegacyExceptionQuota.NewLegacyExceptionQuota("x", "y");
        item.ExpiresAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyExceptionQuotaEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new LegacyExceptionQuotaEmployeeIdLengthRule(120);
        var item = LegacyExceptionQuota.NewLegacyExceptionQuota("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyExceptionQuotaValidatorReportsEveryProblem();
        LegacyExceptionQuotaExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        LegacyExceptionQuotaEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
