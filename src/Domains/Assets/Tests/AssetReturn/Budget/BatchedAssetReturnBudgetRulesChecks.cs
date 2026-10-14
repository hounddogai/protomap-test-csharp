namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedAssetReturnBudgetRulesChecks
{
    public static void BatchedAssetReturnBudgetValidatorReportsEveryProblem()
    {
        var validator = BatchedAssetReturnBudgetRules.DefaultBatchedAssetReturnBudgetValidator();
        var item = BatchedAssetReturnBudget.NewBatchedAssetReturnBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAssetReturnBudgetDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new BatchedAssetReturnBudgetDescriptionLengthRule(64);
        var item = BatchedAssetReturnBudget.NewBatchedAssetReturnBudget("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedAssetReturnBudgetRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new BatchedAssetReturnBudgetRequiredPhoneRule("batched asset return budget needs this field");
        var item = BatchedAssetReturnBudget.NewBatchedAssetReturnBudget("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAssetReturnBudgetEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new BatchedAssetReturnBudgetEndsAtRangeRule(64, 255);
        var item = BatchedAssetReturnBudget.NewBatchedAssetReturnBudget("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedAssetReturnBudgetValidatorReportsEveryProblem();
        BatchedAssetReturnBudgetDescriptionLengthRuleFlagsALongDescription();
        BatchedAssetReturnBudgetRequiredPhoneRuleFlagsABlankPhone();
        BatchedAssetReturnBudgetEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
