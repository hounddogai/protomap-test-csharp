namespace Acme.Workforce.Absence.Tests.Holiday.Quota;

using Acme.Workforce.Absence.Holiday.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class HolidayQuotaRulesChecks
{
    public static void HolidayQuotaValidatorReportsEveryProblem()
    {
        var validator = HolidayQuotaRules.DefaultHolidayQuotaValidator();
        var item = HolidayQuota.NewHolidayQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayQuotaQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new HolidayQuotaQuantityRangeRule(255, 3600);
        var item = HolidayQuota.NewHolidayQuota("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayQuotaRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new HolidayQuotaRequiredContactEmailRule("holiday quota needs this field");
        var item = HolidayQuota.NewHolidayQuota("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayQuotaFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new HolidayQuotaFloorValueRangeRule(8.2, 858.4);
        var item = HolidayQuota.NewHolidayQuota("x", "y");
        item.FloorValue = 859.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HolidayQuotaValidatorReportsEveryProblem();
        HolidayQuotaQuantityRangeRuleFlagsValuesAboveQuantity();
        HolidayQuotaRequiredContactEmailRuleFlagsABlankContactEmail();
        HolidayQuotaFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
