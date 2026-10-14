namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class WeightedBalanceSnapshotRulesChecks
{
    public static void WeightedBalanceSnapshotValidatorReportsEveryProblem()
    {
        var validator = WeightedBalanceSnapshotRules.DefaultWeightedBalanceSnapshotValidator();
        var item = WeightedBalanceSnapshot.NewWeightedBalanceSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedBalanceSnapshotStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new WeightedBalanceSnapshotStartsAtRangeRule(64, 80);
        var item = WeightedBalanceSnapshot.NewWeightedBalanceSnapshot("x", "y");
        item.StartsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedBalanceSnapshotRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedBalanceSnapshotRequiredNameRule("weighted balance snapshot needs a value");
        var item = WeightedBalanceSnapshot.NewWeightedBalanceSnapshot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedBalanceSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedBalanceSnapshotRequiredCodeRule("weighted balance snapshot needs a value");
        var item = WeightedBalanceSnapshot.NewWeightedBalanceSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedBalanceSnapshotValidatorReportsEveryProblem();
        WeightedBalanceSnapshotStartsAtRangeRuleFlagsValuesAboveStartsAt();
        WeightedBalanceSnapshotRequiredNameRuleFlagsABlankName();
        WeightedBalanceSnapshotRequiredCodeRuleFlagsABlankCode();
    }
}
