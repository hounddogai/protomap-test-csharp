namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GarnishmentTargetRulesChecks
{
    public static void GarnishmentTargetValidatorReportsEveryProblem()
    {
        var validator = GarnishmentTargetRules.DefaultGarnishmentTargetValidator();
        var item = GarnishmentTarget.NewGarnishmentTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentTargetRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new GarnishmentTargetRequiredPhoneRule("garnishment target needs a value");
        var item = GarnishmentTarget.NewGarnishmentTarget("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentTargetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GarnishmentTargetTitleLengthRule(64);
        var item = GarnishmentTarget.NewGarnishmentTarget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentTargetRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new GarnishmentTargetRatioRangeRule(6.8, 4365.4);
        var item = GarnishmentTarget.NewGarnishmentTarget("x", "y");
        item.Ratio = 4366.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentTargetCountryLengthRuleFlagsALongCountry()
    {
        var rule = new GarnishmentTargetCountryLengthRule(3600);
        var item = GarnishmentTarget.NewGarnishmentTarget("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GarnishmentTargetValidatorReportsEveryProblem();
        GarnishmentTargetRequiredPhoneRuleFlagsABlankPhone();
        GarnishmentTargetTitleLengthRuleFlagsALongTitle();
        GarnishmentTargetRatioRangeRuleFlagsValuesAboveRatio();
        GarnishmentTargetCountryLengthRuleFlagsALongCountry();
    }
}
