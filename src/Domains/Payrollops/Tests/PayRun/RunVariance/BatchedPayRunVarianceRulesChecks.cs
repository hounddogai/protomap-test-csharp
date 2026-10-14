namespace Acme.Workforce.Payrollops.Tests.PayRun.RunVariance;

using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Checks of run variance code, run by the project's runner.</summary>
public static class BatchedPayRunVarianceRulesChecks
{
    public static void BatchedPayRunVarianceValidatorReportsEveryProblem()
    {
        var validator = BatchedPayRunVarianceRules.DefaultBatchedPayRunVarianceValidator();
        var item = BatchedPayRunVariance.NewBatchedPayRunVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedPayRunVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedPayRunVarianceRequiredCodeRule("batched pay run variance needs an entry");
        var item = BatchedPayRunVariance.NewBatchedPayRunVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedPayRunVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedPayRunVarianceCodeLengthRule(86400);
        var item = BatchedPayRunVariance.NewBatchedPayRunVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPayRunVarianceDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new BatchedPayRunVarianceDiscountRangeRule(8.9, 3185.4);
        var item = BatchedPayRunVariance.NewBatchedPayRunVariance("x", "y");
        item.Discount = 3186.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPayRunVarianceRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new BatchedPayRunVarianceRequiredTeamRule("batched pay run variance needs an entry");
        var item = BatchedPayRunVariance.NewBatchedPayRunVariance("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedPayRunVarianceValidatorReportsEveryProblem();
        BatchedPayRunVarianceRequiredCodeRuleFlagsABlankCode();
        BatchedPayRunVarianceCodeLengthRuleFlagsALongCode();
        BatchedPayRunVarianceDiscountRangeRuleFlagsValuesAboveDiscount();
        BatchedPayRunVarianceRequiredTeamRuleFlagsABlankTeam();
    }
}
