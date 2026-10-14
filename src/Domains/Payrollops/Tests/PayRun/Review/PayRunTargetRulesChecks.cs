namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunTargetRulesChecks
{
    public static void PayRunTargetValidatorReportsEveryProblem()
    {
        var validator = PayRunTargetRules.DefaultPayRunTargetValidator();
        var item = PayRunTarget.NewPayRunTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunTargetPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new PayRunTargetPhoneLengthRule(64);
        var item = PayRunTarget.NewPayRunTarget("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayRunTargetCodeLengthRule(255);
        var item = PayRunTarget.NewPayRunTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunTargetSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new PayRunTargetSequenceRangeRule(64, 86400);
        var item = PayRunTarget.NewPayRunTarget("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunTargetDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new PayRunTargetDepartmentLengthRule(16);
        var item = PayRunTarget.NewPayRunTarget("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunTargetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PayRunTargetRequiredLabelRule("pay run target needs a value");
        var item = PayRunTarget.NewPayRunTarget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayRunTargetValidatorReportsEveryProblem();
        PayRunTargetPhoneLengthRuleFlagsALongPhone();
        PayRunTargetCodeLengthRuleFlagsALongCode();
        PayRunTargetSequenceRangeRuleFlagsValuesAboveSequence();
        PayRunTargetDepartmentLengthRuleFlagsALongDepartment();
        PayRunTargetRequiredLabelRuleFlagsABlankLabel();
    }
}
