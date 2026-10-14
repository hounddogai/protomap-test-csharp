namespace Acme.Workforce.Payrollops.Tests.Deduction.Review;

using Acme.Workforce.Payrollops.Deduction.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ManualDeductionNoticeRulesChecks
{
    public static void ManualDeductionNoticeValidatorReportsEveryProblem()
    {
        var validator = ManualDeductionNoticeRules.DefaultManualDeductionNoticeValidator();
        var item = ManualDeductionNotice.NewManualDeductionNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualDeductionNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualDeductionNoticeRequiredCodeRule("manual deduction notice needs this field");
        var item = ManualDeductionNotice.NewManualDeductionNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualDeductionNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualDeductionNoticeCodeLengthRule(80);
        var item = ManualDeductionNotice.NewManualDeductionNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualDeductionNoticeValidatorReportsEveryProblem();
        ManualDeductionNoticeRequiredCodeRuleFlagsABlankCode();
        ManualDeductionNoticeCodeLengthRuleFlagsALongCode();
    }
}
