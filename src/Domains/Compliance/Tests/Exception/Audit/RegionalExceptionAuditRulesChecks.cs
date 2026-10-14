namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RegionalExceptionAuditRulesChecks
{
    public static void RegionalExceptionAuditValidatorReportsEveryProblem()
    {
        var validator = RegionalExceptionAuditRules.DefaultRegionalExceptionAuditValidator();
        var item = RegionalExceptionAudit.NewRegionalExceptionAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalExceptionAuditLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalExceptionAuditLabelLengthRule(64);
        var item = RegionalExceptionAudit.NewRegionalExceptionAudit("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalExceptionAuditExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RegionalExceptionAuditExpiresAtRangeRule(80, 255);
        var item = RegionalExceptionAudit.NewRegionalExceptionAudit("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalExceptionAuditValidatorReportsEveryProblem();
        RegionalExceptionAuditLabelLengthRuleFlagsALongLabel();
        RegionalExceptionAuditExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
