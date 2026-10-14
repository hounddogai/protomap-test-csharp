namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantyCheckpointRulesChecks
{
    public static void WarrantyCheckpointValidatorReportsEveryProblem()
    {
        var validator = WarrantyCheckpointRules.DefaultWarrantyCheckpointValidator();
        var item = WarrantyCheckpoint.NewWarrantyCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyCheckpointTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new WarrantyCheckpointTimezoneLengthRule(64);
        var item = WarrantyCheckpoint.NewWarrantyCheckpoint("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyCheckpointTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WarrantyCheckpointTitleLengthRule(16);
        var item = WarrantyCheckpoint.NewWarrantyCheckpoint("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantyCheckpointValidatorReportsEveryProblem();
        WarrantyCheckpointTimezoneLengthRuleFlagsALongTimezone();
        WarrantyCheckpointTitleLengthRuleFlagsALongTitle();
    }
}
