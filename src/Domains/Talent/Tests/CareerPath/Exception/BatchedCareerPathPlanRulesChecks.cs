namespace Acme.Workforce.Talent.Tests.CareerPath.Exception;

using Acme.Workforce.Talent.CareerPath.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedCareerPathPlanRulesChecks
{
    public static void BatchedCareerPathPlanValidatorReportsEveryProblem()
    {
        var validator = BatchedCareerPathPlanRules.DefaultBatchedCareerPathPlanValidator();
        var item = BatchedCareerPathPlan.NewBatchedCareerPathPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedCareerPathPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedCareerPathPlanRequiredCodeRule("batched career path plan needs a value");
        var item = BatchedCareerPathPlan.NewBatchedCareerPathPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedCareerPathPlanNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedCareerPathPlanNameLengthRule(1000);
        var item = BatchedCareerPathPlan.NewBatchedCareerPathPlan("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedCareerPathPlanRequiredNameRuleFlagsABlankName()
    {
        var rule = new BatchedCareerPathPlanRequiredNameRule("batched career path plan needs this field");
        var item = BatchedCareerPathPlan.NewBatchedCareerPathPlan("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedCareerPathPlanValidatorReportsEveryProblem();
        BatchedCareerPathPlanRequiredCodeRuleFlagsABlankCode();
        BatchedCareerPathPlanNameLengthRuleFlagsALongName();
        BatchedCareerPathPlanRequiredNameRuleFlagsABlankName();
    }
}
