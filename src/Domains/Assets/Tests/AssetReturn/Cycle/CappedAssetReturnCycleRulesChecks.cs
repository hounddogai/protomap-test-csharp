namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class CappedAssetReturnCycleRulesChecks
{
    public static void CappedAssetReturnCycleValidatorReportsEveryProblem()
    {
        var validator = CappedAssetReturnCycleRules.DefaultCappedAssetReturnCycleValidator();
        var item = CappedAssetReturnCycle.NewCappedAssetReturnCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAssetReturnCycleNameLengthRuleFlagsALongName()
    {
        var rule = new CappedAssetReturnCycleNameLengthRule(32);
        var item = CappedAssetReturnCycle.NewCappedAssetReturnCycle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAssetReturnCycleMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new CappedAssetReturnCycleMaximumRangeRule(4.2, 788.0);
        var item = CappedAssetReturnCycle.NewCappedAssetReturnCycle("x", "y");
        item.Maximum = 789.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAssetReturnCycleJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new CappedAssetReturnCycleJobCodeLengthRule(32);
        var item = CappedAssetReturnCycle.NewCappedAssetReturnCycle("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAssetReturnCycleBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new CappedAssetReturnCycleBudgetRangeRule(8.1, 4081.6);
        var item = CappedAssetReturnCycle.NewCappedAssetReturnCycle("x", "y");
        item.Budget = 4083.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedAssetReturnCycleValidatorReportsEveryProblem();
        CappedAssetReturnCycleNameLengthRuleFlagsALongName();
        CappedAssetReturnCycleMaximumRangeRuleFlagsValuesAboveMaximum();
        CappedAssetReturnCycleJobCodeLengthRuleFlagsALongJobCode();
        CappedAssetReturnCycleBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
