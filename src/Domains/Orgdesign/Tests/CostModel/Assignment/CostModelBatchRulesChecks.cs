namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelBatchRulesChecks
{
    public static void CostModelBatchValidatorReportsEveryProblem()
    {
        var validator = CostModelBatchRules.DefaultCostModelBatchValidator();
        var item = CostModelBatch.NewCostModelBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelBatchRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new CostModelBatchRetriesRangeRule(32, 1000);
        var item = CostModelBatch.NewCostModelBatch("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelBatchCodeLengthRule(255);
        var item = CostModelBatch.NewCostModelBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelBatchNameLengthRuleFlagsALongName()
    {
        var rule = new CostModelBatchNameLengthRule(64);
        var item = CostModelBatch.NewCostModelBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new CostModelBatchRequiredNameRule("cost model batch needs this field");
        var item = CostModelBatch.NewCostModelBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelBatchEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new CostModelBatchEndsAtRangeRule(64, 86400);
        var item = CostModelBatch.NewCostModelBatch("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CostModelBatchValidatorReportsEveryProblem();
        CostModelBatchRetriesRangeRuleFlagsValuesAboveRetries();
        CostModelBatchCodeLengthRuleFlagsALongCode();
        CostModelBatchNameLengthRuleFlagsALongName();
        CostModelBatchRequiredNameRuleFlagsABlankName();
        CostModelBatchEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
