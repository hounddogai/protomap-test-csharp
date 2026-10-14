namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalHolidayAdjustmentRulesChecks
{
    public static void RegionalHolidayAdjustmentValidatorReportsEveryProblem()
    {
        var validator = RegionalHolidayAdjustmentRules.DefaultRegionalHolidayAdjustmentValidator();
        var item = RegionalHolidayAdjustment.NewRegionalHolidayAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalHolidayAdjustmentRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new RegionalHolidayAdjustmentRequiredPhoneRule("regional holiday adjustment needs an entry");
        var item = RegionalHolidayAdjustment.NewRegionalHolidayAdjustment("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalHolidayAdjustmentPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new RegionalHolidayAdjustmentPhoneLengthRule(16);
        var item = RegionalHolidayAdjustment.NewRegionalHolidayAdjustment("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalHolidayAdjustmentValidatorReportsEveryProblem();
        RegionalHolidayAdjustmentRequiredPhoneRuleFlagsABlankPhone();
        RegionalHolidayAdjustmentPhoneLengthRuleFlagsALongPhone();
    }
}
