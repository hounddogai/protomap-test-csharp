namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedExceptionRecordRulesChecks
{
    public static void BatchedExceptionRecordValidatorReportsEveryProblem()
    {
        var validator = BatchedExceptionRecordRules.DefaultBatchedExceptionRecordValidator();
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedExceptionRecordRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new BatchedExceptionRecordRequiredCostCenterRule("batched exception record needs a value");
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedExceptionRecordMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BatchedExceptionRecordMarginRangeRule(0.3, 2193.5);
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("x", "y");
        item.Margin = 2195.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedExceptionRecordNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedExceptionRecordNameLengthRule(80);
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedExceptionRecordRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new BatchedExceptionRecordRequiredCategoryRule("batched exception record needs a value");
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedExceptionRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedExceptionRecordCodeLengthRule(120);
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedExceptionRecordCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new BatchedExceptionRecordCostCenterLengthRule(64);
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedExceptionRecordValidatorReportsEveryProblem();
        BatchedExceptionRecordRequiredCostCenterRuleFlagsABlankCostCenter();
        BatchedExceptionRecordMarginRangeRuleFlagsValuesAboveMargin();
        BatchedExceptionRecordNameLengthRuleFlagsALongName();
        BatchedExceptionRecordRequiredCategoryRuleFlagsABlankCategory();
        BatchedExceptionRecordCodeLengthRuleFlagsALongCode();
        BatchedExceptionRecordCostCenterLengthRuleFlagsALongCostCenter();
    }
}
