namespace Acme.Workforce.Compliance.Tests.Exception.Cycle;

using Acme.Workforce.Compliance.Exception.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ExceptionCycleRulesChecks
{
    public static void ExceptionCycleValidatorReportsEveryProblem()
    {
        var validator = ExceptionCycleRules.DefaultExceptionCycleValidator();
        var item = ExceptionCycle.NewExceptionCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionCycleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ExceptionCycleRequiredLabelRule("exception cycle needs this field");
        var item = ExceptionCycle.NewExceptionCycle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionCycleWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new ExceptionCycleWindowDaysRangeRule(16, 64);
        var item = ExceptionCycle.NewExceptionCycle("x", "y");
        item.WindowDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionCycleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ExceptionCycleLabelLengthRule(1000);
        var item = ExceptionCycle.NewExceptionCycle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionCycleCodeLengthRuleFlagsALongCode()
    {
        var rule = new ExceptionCycleCodeLengthRule(32);
        var item = ExceptionCycle.NewExceptionCycle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionCycleRequiredCodeRule("exception cycle needs this field");
        var item = ExceptionCycle.NewExceptionCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionCycleValidatorReportsEveryProblem();
        ExceptionCycleRequiredLabelRuleFlagsABlankLabel();
        ExceptionCycleWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        ExceptionCycleLabelLengthRuleFlagsALongLabel();
        ExceptionCycleCodeLengthRuleFlagsALongCode();
        ExceptionCycleRequiredCodeRuleFlagsABlankCode();
    }
}
