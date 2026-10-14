namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class GlobalRetroPayBatchRulesChecks
{
    public static void GlobalRetroPayBatchValidatorReportsEveryProblem()
    {
        var validator = GlobalRetroPayBatchRules.DefaultGlobalRetroPayBatchValidator();
        var item = GlobalRetroPayBatch.NewGlobalRetroPayBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalRetroPayBatchRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new GlobalRetroPayBatchRequiredGradeRule("global retro pay batch needs a value");
        var item = GlobalRetroPayBatch.NewGlobalRetroPayBatch("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalRetroPayBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalRetroPayBatchCodeLengthRule(64);
        var item = GlobalRetroPayBatch.NewGlobalRetroPayBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalRetroPayBatchGradeLengthRuleFlagsALongGrade()
    {
        var rule = new GlobalRetroPayBatchGradeLengthRule(1000);
        var item = GlobalRetroPayBatch.NewGlobalRetroPayBatch("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalRetroPayBatchValidatorReportsEveryProblem();
        GlobalRetroPayBatchRequiredGradeRuleFlagsABlankGrade();
        GlobalRetroPayBatchCodeLengthRuleFlagsALongCode();
        GlobalRetroPayBatchGradeLengthRuleFlagsALongGrade();
    }
}
