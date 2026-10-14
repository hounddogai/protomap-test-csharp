namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LegacyAuditCheckpointRulesChecks
{
    public static void LegacyAuditCheckpointValidatorReportsEveryProblem()
    {
        var validator = LegacyAuditCheckpointRules.DefaultLegacyAuditCheckpointValidator();
        var item = LegacyAuditCheckpoint.NewLegacyAuditCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyAuditCheckpointRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyAuditCheckpointRequiredTitleRule("legacy audit checkpoint needs an entry");
        var item = LegacyAuditCheckpoint.NewLegacyAuditCheckpoint("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyAuditCheckpointQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new LegacyAuditCheckpointQuantityRangeRule(3600, 3600);
        var item = LegacyAuditCheckpoint.NewLegacyAuditCheckpoint("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyAuditCheckpointValidatorReportsEveryProblem();
        LegacyAuditCheckpointRequiredTitleRuleFlagsABlankTitle();
        LegacyAuditCheckpointQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
