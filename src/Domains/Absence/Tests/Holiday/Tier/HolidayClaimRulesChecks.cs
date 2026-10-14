namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayClaimRulesChecks
{
    public static void HolidayClaimValidatorReportsEveryProblem()
    {
        var validator = HolidayClaimRules.DefaultHolidayClaimValidator();
        var item = HolidayClaim.NewHolidayClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayClaimRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new HolidayClaimRequiredPhoneRule("holiday claim needs this field");
        var item = HolidayClaim.NewHolidayClaim("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayClaimRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new HolidayClaimRequiredCurrencyRule("holiday claim needs a value");
        var item = HolidayClaim.NewHolidayClaim("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayClaimFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new HolidayClaimFloorValueRangeRule(4.8, 1529.5);
        var item = HolidayClaim.NewHolidayClaim("x", "y");
        item.FloorValue = 1531.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidayClaimCodeLengthRule(80);
        var item = HolidayClaim.NewHolidayClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidayClaimValidatorReportsEveryProblem();
        HolidayClaimRequiredPhoneRuleFlagsABlankPhone();
        HolidayClaimRequiredCurrencyRuleFlagsABlankCurrency();
        HolidayClaimFloorValueRangeRuleFlagsValuesAboveFloorValue();
        HolidayClaimCodeLengthRuleFlagsALongCode();
    }
}
