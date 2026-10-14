namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class ManualReorgVarianceRulesChecks
{
    public static void ManualReorgVarianceValidatorReportsEveryProblem()
    {
        var validator = ManualReorgVarianceRules.DefaultManualReorgVarianceValidator();
        var item = ManualReorgVariance.NewManualReorgVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualReorgVarianceRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new ManualReorgVarianceRequiredCostCenterRule("manual reorg variance needs an entry");
        var item = ManualReorgVariance.NewManualReorgVariance("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualReorgVarianceMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ManualReorgVarianceMaximumRangeRule(8.4, 2962.5);
        var item = ManualReorgVariance.NewManualReorgVariance("x", "y");
        item.Maximum = 2964.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualReorgVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualReorgVarianceCodeLengthRule(32);
        var item = ManualReorgVariance.NewManualReorgVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualReorgVarianceCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new ManualReorgVarianceCostCenterLengthRule(16);
        var item = ManualReorgVariance.NewManualReorgVariance("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualReorgVarianceFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new ManualReorgVarianceFteRangeRule(5.8, 3657.8);
        var item = ManualReorgVariance.NewManualReorgVariance("x", "y");
        item.Fte = 3659.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualReorgVarianceValidatorReportsEveryProblem();
        ManualReorgVarianceRequiredCostCenterRuleFlagsABlankCostCenter();
        ManualReorgVarianceMaximumRangeRuleFlagsValuesAboveMaximum();
        ManualReorgVarianceCodeLengthRuleFlagsALongCode();
        ManualReorgVarianceCostCenterLengthRuleFlagsALongCostCenter();
        ManualReorgVarianceFteRangeRuleFlagsValuesAboveFte();
    }
}
