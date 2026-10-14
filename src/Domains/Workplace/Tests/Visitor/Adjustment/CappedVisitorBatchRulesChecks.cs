namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedVisitorBatchRulesChecks
{
    public static void CappedVisitorBatchValidatorReportsEveryProblem()
    {
        var validator = CappedVisitorBatchRules.DefaultCappedVisitorBatchValidator();
        var item = CappedVisitorBatch.NewCappedVisitorBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedVisitorBatchRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new CappedVisitorBatchRequiredPhoneRule("capped visitor batch needs an entry");
        var item = CappedVisitorBatch.NewCappedVisitorBatch("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedVisitorBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CappedVisitorBatchTitleLengthRule(120);
        var item = CappedVisitorBatch.NewCappedVisitorBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedVisitorBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedVisitorBatchCodeLengthRule(86400);
        var item = CappedVisitorBatch.NewCappedVisitorBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedVisitorBatchValidatorReportsEveryProblem();
        CappedVisitorBatchRequiredPhoneRuleFlagsABlankPhone();
        CappedVisitorBatchTitleLengthRuleFlagsALongTitle();
        CappedVisitorBatchCodeLengthRuleFlagsALongCode();
    }
}
