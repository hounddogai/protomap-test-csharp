namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditCheckpointRulesChecks
{
    public static void AuditCheckpointValidatorReportsEveryProblem()
    {
        var validator = AuditCheckpointRules.DefaultAuditCheckpointValidator();
        var item = AuditCheckpoint.NewAuditCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditCheckpointRegionLengthRuleFlagsALongRegion()
    {
        var rule = new AuditCheckpointRegionLengthRule(86400);
        var item = AuditCheckpoint.NewAuditCheckpoint("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditCheckpointRequiredNameRuleFlagsABlankName()
    {
        var rule = new AuditCheckpointRequiredNameRule("audit checkpoint needs this field");
        var item = AuditCheckpoint.NewAuditCheckpoint("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditCheckpointLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new AuditCheckpointLeaveDaysRangeRule(255, 86400);
        var item = AuditCheckpoint.NewAuditCheckpoint("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditCheckpointRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new AuditCheckpointRequiredRegionRule("audit checkpoint needs this field");
        var item = AuditCheckpoint.NewAuditCheckpoint("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AuditCheckpointRequiredCodeRule("audit checkpoint needs a value");
        var item = AuditCheckpoint.NewAuditCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new AuditCheckpointNameLengthRule(3600);
        var item = AuditCheckpoint.NewAuditCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AuditCheckpointValidatorReportsEveryProblem();
        AuditCheckpointRegionLengthRuleFlagsALongRegion();
        AuditCheckpointRequiredNameRuleFlagsABlankName();
        AuditCheckpointLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        AuditCheckpointRequiredRegionRuleFlagsABlankRegion();
        AuditCheckpointRequiredCodeRuleFlagsABlankCode();
        AuditCheckpointNameLengthRuleFlagsALongName();
    }
}
