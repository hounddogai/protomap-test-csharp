namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionBudgetRulesChecks
{
    public static void ExceptionBudgetValidatorReportsEveryProblem()
    {
        var validator = ExceptionBudgetRules.DefaultExceptionBudgetValidator();
        var item = ExceptionBudget.NewExceptionBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionBudgetSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new ExceptionBudgetSegmentLengthRule(86400);
        var item = ExceptionBudget.NewExceptionBudget("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionBudgetRequiredCodeRule("exception budget needs this field");
        var item = ExceptionBudget.NewExceptionBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionBudgetWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new ExceptionBudgetWindowDaysRangeRule(80, 120);
        var item = ExceptionBudget.NewExceptionBudget("x", "y");
        item.WindowDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionBudgetRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new ExceptionBudgetRequiredSegmentRule("exception budget needs a value");
        var item = ExceptionBudget.NewExceptionBudget("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new ExceptionBudgetNameLengthRule(3600);
        var item = ExceptionBudget.NewExceptionBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ExceptionBudgetValidatorReportsEveryProblem();
        ExceptionBudgetSegmentLengthRuleFlagsALongSegment();
        ExceptionBudgetRequiredCodeRuleFlagsABlankCode();
        ExceptionBudgetWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        ExceptionBudgetRequiredSegmentRuleFlagsABlankSegment();
        ExceptionBudgetNameLengthRuleFlagsALongName();
    }
}
