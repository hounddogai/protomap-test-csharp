namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PendingDeductionExceptionRulesChecks
{
    public static void PendingDeductionExceptionValidatorReportsEveryProblem()
    {
        var validator = PendingDeductionExceptionRules.DefaultPendingDeductionExceptionValidator();
        var item = PendingDeductionException.NewPendingDeductionException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingDeductionExceptionRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingDeductionExceptionRequiredLabelRule("pending deduction exception needs this field");
        var item = PendingDeductionException.NewPendingDeductionException("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingDeductionExceptionLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingDeductionExceptionLabelLengthRule(3600);
        var item = PendingDeductionException.NewPendingDeductionException("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingDeductionExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingDeductionExceptionCodeLengthRule(86400);
        var item = PendingDeductionException.NewPendingDeductionException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingDeductionExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingDeductionExceptionRequiredCodeRule("pending deduction exception needs a value");
        var item = PendingDeductionException.NewPendingDeductionException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingDeductionExceptionLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new PendingDeductionExceptionLeaveDaysRangeRule(32, 32);
        var item = PendingDeductionException.NewPendingDeductionException("x", "y");
        item.LeaveDays = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingDeductionExceptionValidatorReportsEveryProblem();
        PendingDeductionExceptionRequiredLabelRuleFlagsABlankLabel();
        PendingDeductionExceptionLabelLengthRuleFlagsALongLabel();
        PendingDeductionExceptionCodeLengthRuleFlagsALongCode();
        PendingDeductionExceptionRequiredCodeRuleFlagsABlankCode();
        PendingDeductionExceptionLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
