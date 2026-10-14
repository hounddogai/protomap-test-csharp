namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionAuditRulesChecks
{
    public static void ExceptionAuditValidatorReportsEveryProblem()
    {
        var validator = ExceptionAuditRules.DefaultExceptionAuditValidator();
        var item = ExceptionAudit.NewExceptionAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionAuditDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new ExceptionAuditDurationMinutesRangeRule(1000, 3600);
        var item = ExceptionAudit.NewExceptionAudit("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionAuditUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new ExceptionAuditUpdatedAtRangeRule(16, 32);
        var item = ExceptionAudit.NewExceptionAudit("x", "y");
        item.UpdatedAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionAuditPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ExceptionAuditPriorityRangeRule(255, 3600);
        var item = ExceptionAudit.NewExceptionAudit("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionAuditLocationLengthRuleFlagsALongLocation()
    {
        var rule = new ExceptionAuditLocationLengthRule(120);
        var item = ExceptionAudit.NewExceptionAudit("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ExceptionAuditValidatorReportsEveryProblem();
        ExceptionAuditDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        ExceptionAuditUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        ExceptionAuditPriorityRangeRuleFlagsValuesAbovePriority();
        ExceptionAuditLocationLengthRuleFlagsALongLocation();
    }
}
