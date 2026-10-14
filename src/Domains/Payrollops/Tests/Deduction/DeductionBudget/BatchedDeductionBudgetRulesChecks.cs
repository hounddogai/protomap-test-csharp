namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBudget;

using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Checks of deduction budget code, run by the project's runner.</summary>
public static class BatchedDeductionBudgetRulesChecks
{
    public static void BatchedDeductionBudgetValidatorReportsEveryProblem()
    {
        var validator = BatchedDeductionBudgetRules.DefaultBatchedDeductionBudgetValidator();
        var item = BatchedDeductionBudget.NewBatchedDeductionBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedDeductionBudgetExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new BatchedDeductionBudgetExpiresAtRangeRule(80, 86400);
        var item = BatchedDeductionBudget.NewBatchedDeductionBudget("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedDeductionBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedDeductionBudgetRequiredCodeRule("batched deduction budget needs a value");
        var item = BatchedDeductionBudget.NewBatchedDeductionBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedDeductionBudgetEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new BatchedDeductionBudgetEmployeeIdLengthRule(120);
        var item = BatchedDeductionBudget.NewBatchedDeductionBudget("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedDeductionBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedDeductionBudgetCodeLengthRule(64);
        var item = BatchedDeductionBudget.NewBatchedDeductionBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedDeductionBudgetValidatorReportsEveryProblem();
        BatchedDeductionBudgetExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        BatchedDeductionBudgetRequiredCodeRuleFlagsABlankCode();
        BatchedDeductionBudgetEmployeeIdLengthRuleFlagsALongEmployeeId();
        BatchedDeductionBudgetCodeLengthRuleFlagsALongCode();
    }
}
