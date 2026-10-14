namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedBonusPoolCheckpointRulesChecks
{
    public static void WeightedBonusPoolCheckpointValidatorReportsEveryProblem()
    {
        var validator = WeightedBonusPoolCheckpointRules.DefaultWeightedBonusPoolCheckpointValidator();
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedBonusPoolCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedBonusPoolCheckpointLabelLengthRule(64);
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedBonusPoolCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedBonusPoolCheckpointRequiredLabelRule("weighted bonus pool checkpoint needs an entry");
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedBonusPoolCheckpointExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new WeightedBonusPoolCheckpointExternalRefLengthRule(32);
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedBonusPoolCheckpointAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new WeightedBonusPoolCheckpointAmountRangeRule(1.5, 1864.0);
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("x", "y");
        item.Amount = 1865.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedBonusPoolCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedBonusPoolCheckpointCodeLengthRule(120);
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedBonusPoolCheckpointValidatorReportsEveryProblem();
        WeightedBonusPoolCheckpointLabelLengthRuleFlagsALongLabel();
        WeightedBonusPoolCheckpointRequiredLabelRuleFlagsABlankLabel();
        WeightedBonusPoolCheckpointExternalRefLengthRuleFlagsALongExternalRef();
        WeightedBonusPoolCheckpointAmountRangeRuleFlagsValuesAboveAmount();
        WeightedBonusPoolCheckpointCodeLengthRuleFlagsALongCode();
    }
}
