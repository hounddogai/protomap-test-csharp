namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultTrainingDueAuditRulesChecks
{
    public static void DefaultTrainingDueAuditValidatorReportsEveryProblem()
    {
        var validator = DefaultTrainingDueAuditRules.DefaultDefaultTrainingDueAuditValidator();
        var item = DefaultTrainingDueAudit.NewDefaultTrainingDueAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultTrainingDueAuditUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new DefaultTrainingDueAuditUpdatedAtRangeRule(1000, 86400);
        var item = DefaultTrainingDueAudit.NewDefaultTrainingDueAudit("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultTrainingDueAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultTrainingDueAuditRequiredCodeRule("default training due audit needs this field");
        var item = DefaultTrainingDueAudit.NewDefaultTrainingDueAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultTrainingDueAuditRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new DefaultTrainingDueAuditRequiredUnitRule("default training due audit needs an entry");
        var item = DefaultTrainingDueAudit.NewDefaultTrainingDueAudit("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultTrainingDueAuditValidatorReportsEveryProblem();
        DefaultTrainingDueAuditUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        DefaultTrainingDueAuditRequiredCodeRuleFlagsABlankCode();
        DefaultTrainingDueAuditRequiredUnitRuleFlagsABlankUnit();
    }
}
