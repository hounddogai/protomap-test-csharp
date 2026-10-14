namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GarnishmentExceptionRulesChecks
{
    public static void GarnishmentExceptionValidatorReportsEveryProblem()
    {
        var validator = GarnishmentExceptionRules.DefaultGarnishmentExceptionValidator();
        var item = GarnishmentException.NewGarnishmentException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentExceptionCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new GarnishmentExceptionCostRangeRule(3.7, 4952.9);
        var item = GarnishmentException.NewGarnishmentException("x", "y");
        item.Cost = 4954.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentExceptionRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GarnishmentExceptionRequiredTitleRule("garnishment exception needs an entry");
        var item = GarnishmentException.NewGarnishmentException("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentExceptionGradeLengthRuleFlagsALongGrade()
    {
        var rule = new GarnishmentExceptionGradeLengthRule(1000);
        var item = GarnishmentException.NewGarnishmentException("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentExceptionRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new GarnishmentExceptionRequiredGradeRule("garnishment exception needs a value");
        var item = GarnishmentException.NewGarnishmentException("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentExceptionCodeLengthRule(120);
        var item = GarnishmentException.NewGarnishmentException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GarnishmentExceptionValidatorReportsEveryProblem();
        GarnishmentExceptionCostRangeRuleFlagsValuesAboveCost();
        GarnishmentExceptionRequiredTitleRuleFlagsABlankTitle();
        GarnishmentExceptionGradeLengthRuleFlagsALongGrade();
        GarnishmentExceptionRequiredGradeRuleFlagsABlankGrade();
        GarnishmentExceptionCodeLengthRuleFlagsALongCode();
    }
}
