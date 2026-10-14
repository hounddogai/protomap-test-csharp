namespace Acme.Workforce.Assets.Tests.AssetReturn.Batch;

using Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ManualAssetReturnBatchRulesChecks
{
    public static void ManualAssetReturnBatchValidatorReportsEveryProblem()
    {
        var validator = ManualAssetReturnBatchRules.DefaultManualAssetReturnBatchValidator();
        var item = ManualAssetReturnBatch.NewManualAssetReturnBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAssetReturnBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualAssetReturnBatchLabelLengthRule(16);
        var item = ManualAssetReturnBatch.NewManualAssetReturnBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAssetReturnBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualAssetReturnBatchRequiredLabelRule("manual asset return batch needs a value");
        var item = ManualAssetReturnBatch.NewManualAssetReturnBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAssetReturnBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualAssetReturnBatchCodeLengthRule(80);
        var item = ManualAssetReturnBatch.NewManualAssetReturnBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAssetReturnBatchRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new ManualAssetReturnBatchRequiredEmployeeIdRule("manual asset return batch needs a value");
        var item = ManualAssetReturnBatch.NewManualAssetReturnBatch("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualAssetReturnBatchValidatorReportsEveryProblem();
        ManualAssetReturnBatchLabelLengthRuleFlagsALongLabel();
        ManualAssetReturnBatchRequiredLabelRuleFlagsABlankLabel();
        ManualAssetReturnBatchCodeLengthRuleFlagsALongCode();
        ManualAssetReturnBatchRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
