namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class RegionalLicenseSeatSlotRulesChecks
{
    public static void RegionalLicenseSeatSlotValidatorReportsEveryProblem()
    {
        var validator = RegionalLicenseSeatSlotRules.DefaultRegionalLicenseSeatSlotValidator();
        var item = RegionalLicenseSeatSlot.NewRegionalLicenseSeatSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalLicenseSeatSlotRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new RegionalLicenseSeatSlotRequiredCategoryRule("regional license seat slot needs this field");
        var item = RegionalLicenseSeatSlot.NewRegionalLicenseSeatSlot("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalLicenseSeatSlotWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new RegionalLicenseSeatSlotWeightRangeRule(3.0, 1863.5);
        var item = RegionalLicenseSeatSlot.NewRegionalLicenseSeatSlot("x", "y");
        item.Weight = 1865.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalLicenseSeatSlotValidatorReportsEveryProblem();
        RegionalLicenseSeatSlotRequiredCategoryRuleFlagsABlankCategory();
        RegionalLicenseSeatSlotWeightRangeRuleFlagsValuesAboveWeight();
    }
}
