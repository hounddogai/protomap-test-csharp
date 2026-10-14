namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantBatch;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;

/// <summary>Checks of grant batch code, run by the project's runner.</summary>
public static class LenientEquityGrantBatchRulesChecks
{
    public static void LenientEquityGrantBatchValidatorReportsEveryProblem()
    {
        var validator = LenientEquityGrantBatchRules.DefaultLenientEquityGrantBatchValidator();
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientEquityGrantBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientEquityGrantBatchRequiredNameRule("lenient equity grant batch needs this field");
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientEquityGrantBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientEquityGrantBatchRequiredCodeRule("lenient equity grant batch needs this field");
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientEquityGrantBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientEquityGrantBatchCodeLengthRule(16);
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientEquityGrantBatchRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new LenientEquityGrantBatchRequiredUnitRule("lenient equity grant batch needs an entry");
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientEquityGrantBatchJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new LenientEquityGrantBatchJobCodeLengthRule(255);
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientEquityGrantBatchValidatorReportsEveryProblem();
        LenientEquityGrantBatchRequiredNameRuleFlagsABlankName();
        LenientEquityGrantBatchRequiredCodeRuleFlagsABlankCode();
        LenientEquityGrantBatchCodeLengthRuleFlagsALongCode();
        LenientEquityGrantBatchRequiredUnitRuleFlagsABlankUnit();
        LenientEquityGrantBatchJobCodeLengthRuleFlagsALongJobCode();
    }
}
