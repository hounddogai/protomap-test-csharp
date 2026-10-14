namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolBatch;

using Acme.Workforce.Compensation.BonusPool.PoolBatch;

/// <summary>Checks of pool batch code, run by the project's runner.</summary>
public static class TieredBonusPoolBatchRulesChecks
{
    public static void TieredBonusPoolBatchValidatorReportsEveryProblem()
    {
        var validator = TieredBonusPoolBatchRules.DefaultTieredBonusPoolBatchValidator();
        var item = TieredBonusPoolBatch.NewTieredBonusPoolBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredBonusPoolBatchRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new TieredBonusPoolBatchRequiredCostCenterRule("tiered bonus pool batch needs this field");
        var item = TieredBonusPoolBatch.NewTieredBonusPoolBatch("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredBonusPoolBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredBonusPoolBatchCodeLengthRule(86400);
        var item = TieredBonusPoolBatch.NewTieredBonusPoolBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredBonusPoolBatchNameLengthRuleFlagsALongName()
    {
        var rule = new TieredBonusPoolBatchNameLengthRule(16);
        var item = TieredBonusPoolBatch.NewTieredBonusPoolBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredBonusPoolBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredBonusPoolBatchRequiredNameRule("tiered bonus pool batch needs a value");
        var item = TieredBonusPoolBatch.NewTieredBonusPoolBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredBonusPoolBatchValidatorReportsEveryProblem();
        TieredBonusPoolBatchRequiredCostCenterRuleFlagsABlankCostCenter();
        TieredBonusPoolBatchCodeLengthRuleFlagsALongCode();
        TieredBonusPoolBatchNameLengthRuleFlagsALongName();
        TieredBonusPoolBatchRequiredNameRuleFlagsABlankName();
    }
}
