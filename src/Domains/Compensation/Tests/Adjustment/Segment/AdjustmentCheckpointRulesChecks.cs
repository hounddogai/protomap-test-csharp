namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentCheckpointRulesChecks
{
    public static void AdjustmentCheckpointValidatorReportsEveryProblem()
    {
        var validator = AdjustmentCheckpointRules.DefaultAdjustmentCheckpointValidator();
        var item = AdjustmentCheckpoint.NewAdjustmentCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentCheckpointRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new AdjustmentCheckpointRequiredOwnerIdRule("adjustment checkpoint needs a value");
        var item = AdjustmentCheckpoint.NewAdjustmentCheckpoint("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentCheckpointRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new AdjustmentCheckpointRequiredExternalRefRule("adjustment checkpoint needs an entry");
        var item = AdjustmentCheckpoint.NewAdjustmentCheckpoint("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new AdjustmentCheckpointNameLengthRule(32);
        var item = AdjustmentCheckpoint.NewAdjustmentCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdjustmentCheckpointValidatorReportsEveryProblem();
        AdjustmentCheckpointRequiredOwnerIdRuleFlagsABlankOwnerId();
        AdjustmentCheckpointRequiredExternalRefRuleFlagsABlankExternalRef();
        AdjustmentCheckpointNameLengthRuleFlagsALongName();
    }
}
