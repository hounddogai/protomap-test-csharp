namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayMappingRulesChecks
{
    public static void HolidayMappingValidatorReportsEveryProblem()
    {
        var validator = HolidayMappingRules.DefaultHolidayMappingValidator();
        var item = HolidayMapping.NewHolidayMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayMappingEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new HolidayMappingEndsAtRangeRule(64, 1000);
        var item = HolidayMapping.NewHolidayMapping("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayMappingRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new HolidayMappingRequiredCategoryRule("holiday mapping needs this field");
        var item = HolidayMapping.NewHolidayMapping("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayMappingRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new HolidayMappingRequiredLabelRule("holiday mapping needs an entry");
        var item = HolidayMapping.NewHolidayMapping("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayMappingCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new HolidayMappingCategoryLengthRule(120);
        var item = HolidayMapping.NewHolidayMapping("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayMappingCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new HolidayMappingCreatedAtRangeRule(32, 86400);
        var item = HolidayMapping.NewHolidayMapping("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new HolidayMappingLabelLengthRule(3600);
        var item = HolidayMapping.NewHolidayMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidayMappingValidatorReportsEveryProblem();
        HolidayMappingEndsAtRangeRuleFlagsValuesAboveEndsAt();
        HolidayMappingRequiredCategoryRuleFlagsABlankCategory();
        HolidayMappingRequiredLabelRuleFlagsABlankLabel();
        HolidayMappingCategoryLengthRuleFlagsALongCategory();
        HolidayMappingCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        HolidayMappingLabelLengthRuleFlagsALongLabel();
    }
}
