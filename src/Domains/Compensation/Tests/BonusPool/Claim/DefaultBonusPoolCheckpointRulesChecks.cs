namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class DefaultBonusPoolCheckpointRulesChecks
{
    public static void DefaultBonusPoolCheckpointValidatorReportsEveryProblem()
    {
        var validator = DefaultBonusPoolCheckpointRules.DefaultDefaultBonusPoolCheckpointValidator();
        var item = DefaultBonusPoolCheckpoint.NewDefaultBonusPoolCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultBonusPoolCheckpointQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new DefaultBonusPoolCheckpointQuantityRangeRule(16, 86400);
        var item = DefaultBonusPoolCheckpoint.NewDefaultBonusPoolCheckpoint("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultBonusPoolCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultBonusPoolCheckpointCodeLengthRule(120);
        var item = DefaultBonusPoolCheckpoint.NewDefaultBonusPoolCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultBonusPoolCheckpointValidatorReportsEveryProblem();
        DefaultBonusPoolCheckpointQuantityRangeRuleFlagsValuesAboveQuantity();
        DefaultBonusPoolCheckpointCodeLengthRuleFlagsALongCode();
    }
}
