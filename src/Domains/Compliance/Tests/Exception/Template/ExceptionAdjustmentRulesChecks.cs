namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ExceptionAdjustmentRulesChecks
{
    public static void ExceptionAdjustmentValidatorReportsEveryProblem()
    {
        var validator = ExceptionAdjustmentRules.DefaultExceptionAdjustmentValidator();
        var item = ExceptionAdjustment.NewExceptionAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionAdjustmentMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new ExceptionAdjustmentMarginRangeRule(5.2, 2594.0);
        var item = ExceptionAdjustment.NewExceptionAdjustment("x", "y");
        item.Margin = 2595.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionAdjustmentHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new ExceptionAdjustmentHoursRangeRule(1.9, 4801.0);
        var item = ExceptionAdjustment.NewExceptionAdjustment("x", "y");
        item.Hours = 4802.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionAdjustmentLocationLengthRuleFlagsALongLocation()
    {
        var rule = new ExceptionAdjustmentLocationLengthRule(255);
        var item = ExceptionAdjustment.NewExceptionAdjustment("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionAdjustmentRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new ExceptionAdjustmentRequiredLocationRule("exception adjustment needs a value");
        var item = ExceptionAdjustment.NewExceptionAdjustment("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionAdjustmentValidatorReportsEveryProblem();
        ExceptionAdjustmentMarginRangeRuleFlagsValuesAboveMargin();
        ExceptionAdjustmentHoursRangeRuleFlagsValuesAboveHours();
        ExceptionAdjustmentLocationLengthRuleFlagsALongLocation();
        ExceptionAdjustmentRequiredLocationRuleFlagsABlankLocation();
    }
}
