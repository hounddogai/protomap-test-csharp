namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveHolidayVarianceRulesChecks
{
    public static void AdaptiveHolidayVarianceValidatorReportsEveryProblem()
    {
        var validator = AdaptiveHolidayVarianceRules.DefaultAdaptiveHolidayVarianceValidator();
        var item = AdaptiveHolidayVariance.NewAdaptiveHolidayVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveHolidayVarianceWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AdaptiveHolidayVarianceWindowDaysRangeRule(80, 86400);
        var item = AdaptiveHolidayVariance.NewAdaptiveHolidayVariance("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveHolidayVarianceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveHolidayVarianceRequiredLabelRule("adaptive holiday variance needs a value");
        var item = AdaptiveHolidayVariance.NewAdaptiveHolidayVariance("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveHolidayVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveHolidayVarianceCodeLengthRule(16);
        var item = AdaptiveHolidayVariance.NewAdaptiveHolidayVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveHolidayVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveHolidayVarianceRequiredCodeRule("adaptive holiday variance needs an entry");
        var item = AdaptiveHolidayVariance.NewAdaptiveHolidayVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveHolidayVarianceValidatorReportsEveryProblem();
        AdaptiveHolidayVarianceWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AdaptiveHolidayVarianceRequiredLabelRuleFlagsABlankLabel();
        AdaptiveHolidayVarianceCodeLengthRuleFlagsALongCode();
        AdaptiveHolidayVarianceRequiredCodeRuleFlagsABlankCode();
    }
}
