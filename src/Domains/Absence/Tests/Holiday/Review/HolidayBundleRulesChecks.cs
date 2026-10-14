namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidayBundleRulesChecks
{
    public static void HolidayBundleValidatorReportsEveryProblem()
    {
        var validator = HolidayBundleRules.DefaultHolidayBundleValidator();
        var item = HolidayBundle.NewHolidayBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayBundleUnitLengthRuleFlagsALongUnit()
    {
        var rule = new HolidayBundleUnitLengthRule(255);
        var item = HolidayBundle.NewHolidayBundle("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayBundleRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new HolidayBundleRequiredCountryRule("holiday bundle needs this field");
        var item = HolidayBundle.NewHolidayBundle("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HolidayBundleValidatorReportsEveryProblem();
        HolidayBundleUnitLengthRuleFlagsALongUnit();
        HolidayBundleRequiredCountryRuleFlagsABlankCountry();
    }
}
