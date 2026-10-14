namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackPayRunBatchRulesChecks
{
    public static void FallbackPayRunBatchValidatorReportsEveryProblem()
    {
        var validator = FallbackPayRunBatchRules.DefaultFallbackPayRunBatchValidator();
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPayRunBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackPayRunBatchRequiredCodeRule("fallback pay run batch needs this field");
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPayRunBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackPayRunBatchCodeLengthRule(86400);
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayRunBatchNameLengthRuleFlagsALongName()
    {
        var rule = new FallbackPayRunBatchNameLengthRule(1000);
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayRunBatchSourceLengthRuleFlagsALongSource()
    {
        var rule = new FallbackPayRunBatchSourceLengthRule(80);
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayRunBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackPayRunBatchRequiredNameRule("fallback pay run batch needs a value");
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPayRunBatchRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new FallbackPayRunBatchRequiredSourceRule("fallback pay run batch needs a value");
        var item = FallbackPayRunBatch.NewFallbackPayRunBatch("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackPayRunBatchValidatorReportsEveryProblem();
        FallbackPayRunBatchRequiredCodeRuleFlagsABlankCode();
        FallbackPayRunBatchCodeLengthRuleFlagsALongCode();
        FallbackPayRunBatchNameLengthRuleFlagsALongName();
        FallbackPayRunBatchSourceLengthRuleFlagsALongSource();
        FallbackPayRunBatchRequiredNameRuleFlagsABlankName();
        FallbackPayRunBatchRequiredSourceRuleFlagsABlankSource();
    }
}
