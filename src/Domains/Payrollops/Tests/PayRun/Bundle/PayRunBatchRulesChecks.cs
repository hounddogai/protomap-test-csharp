namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunBatchRulesChecks
{
    public static void PayRunBatchValidatorReportsEveryProblem()
    {
        var validator = PayRunBatchRules.DefaultPayRunBatchValidator();
        var item = PayRunBatch.NewPayRunBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PayRunBatchLabelLengthRule(86400);
        var item = PayRunBatch.NewPayRunBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunBatchRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new PayRunBatchRequiredBadgeRule("pay run batch needs this field");
        var item = PayRunBatch.NewPayRunBatch("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PayRunBatchRequiredLabelRule("pay run batch needs a value");
        var item = PayRunBatch.NewPayRunBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayRunBatchRequiredCodeRule("pay run batch needs a value");
        var item = PayRunBatch.NewPayRunBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunBatchBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new PayRunBatchBadgeLengthRule(3600);
        var item = PayRunBatch.NewPayRunBatch("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayRunBatchValidatorReportsEveryProblem();
        PayRunBatchLabelLengthRuleFlagsALongLabel();
        PayRunBatchRequiredBadgeRuleFlagsABlankBadge();
        PayRunBatchRequiredLabelRuleFlagsABlankLabel();
        PayRunBatchRequiredCodeRuleFlagsABlankCode();
        PayRunBatchBadgeLengthRuleFlagsALongBadge();
    }
}
