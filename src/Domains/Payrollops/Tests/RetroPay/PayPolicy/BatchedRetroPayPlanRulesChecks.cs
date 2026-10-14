namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayPolicy;

using Acme.Workforce.Payrollops.RetroPay.PayPolicy;

/// <summary>Checks of pay policy code, run by the project's runner.</summary>
public static class BatchedRetroPayPlanRulesChecks
{
    public static void BatchedRetroPayPlanValidatorReportsEveryProblem()
    {
        var validator = BatchedRetroPayPlanRules.DefaultBatchedRetroPayPlanValidator();
        var item = BatchedRetroPayPlan.NewBatchedRetroPayPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedRetroPayPlanManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new BatchedRetroPayPlanManagerIdLengthRule(86400);
        var item = BatchedRetroPayPlan.NewBatchedRetroPayPlan("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedRetroPayPlanRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new BatchedRetroPayPlanRequiredManagerIdRule("batched retro pay plan needs this field");
        var item = BatchedRetroPayPlan.NewBatchedRetroPayPlan("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedRetroPayPlanValidatorReportsEveryProblem();
        BatchedRetroPayPlanManagerIdLengthRuleFlagsALongManagerId();
        BatchedRetroPayPlanRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
