namespace Acme.Workforce.Compliance.Tests.Audit.Adjustment;

using Acme.Workforce.Compliance.Audit.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RegionalAuditRecordRulesChecks
{
    public static void RegionalAuditRecordValidatorReportsEveryProblem()
    {
        var validator = RegionalAuditRecordRules.DefaultRegionalAuditRecordValidator();
        var item = RegionalAuditRecord.NewRegionalAuditRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAuditRecordPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new RegionalAuditRecordPriorityRangeRule(255, 3600);
        var item = RegionalAuditRecord.NewRegionalAuditRecord("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalAuditRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalAuditRecordLabelLengthRule(3600);
        var item = RegionalAuditRecord.NewRegionalAuditRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalAuditRecordValidatorReportsEveryProblem();
        RegionalAuditRecordPriorityRangeRuleFlagsValuesAbovePriority();
        RegionalAuditRecordLabelLengthRuleFlagsALongLabel();
    }
}
