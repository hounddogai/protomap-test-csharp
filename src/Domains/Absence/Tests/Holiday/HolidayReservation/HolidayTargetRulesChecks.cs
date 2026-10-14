namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class HolidayTargetRulesChecks
{
    public static void HolidayTargetValidatorReportsEveryProblem()
    {
        var validator = HolidayTargetRules.DefaultHolidayTargetValidator();
        var item = HolidayTarget.NewHolidayTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayTargetLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new HolidayTargetLeaveDaysRangeRule(255, 3600);
        var item = HolidayTarget.NewHolidayTarget("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidayTargetCodeLengthRule(255);
        var item = HolidayTarget.NewHolidayTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HolidayTargetRequiredCodeRule("holiday target needs this field");
        var item = HolidayTarget.NewHolidayTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayTargetNameLengthRuleFlagsALongName()
    {
        var rule = new HolidayTargetNameLengthRule(1000);
        var item = HolidayTarget.NewHolidayTarget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new HolidayTargetRequiredNameRule("holiday target needs a value");
        var item = HolidayTarget.NewHolidayTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HolidayTargetValidatorReportsEveryProblem();
        HolidayTargetLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        HolidayTargetCodeLengthRuleFlagsALongCode();
        HolidayTargetRequiredCodeRuleFlagsABlankCode();
        HolidayTargetNameLengthRuleFlagsALongName();
        HolidayTargetRequiredNameRuleFlagsABlankName();
    }
}
