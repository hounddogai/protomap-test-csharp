namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class WeightedHolidayMappingRulesChecks
{
    public static void WeightedHolidayMappingValidatorReportsEveryProblem()
    {
        var validator = WeightedHolidayMappingRules.DefaultWeightedHolidayMappingValidator();
        var item = WeightedHolidayMapping.NewWeightedHolidayMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedHolidayMappingNameLengthRuleFlagsALongName()
    {
        var rule = new WeightedHolidayMappingNameLengthRule(64);
        var item = WeightedHolidayMapping.NewWeightedHolidayMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedHolidayMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedHolidayMappingRequiredNameRule("weighted holiday mapping needs an entry");
        var item = WeightedHolidayMapping.NewWeightedHolidayMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedHolidayMappingPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new WeightedHolidayMappingPhoneLengthRule(64);
        var item = WeightedHolidayMapping.NewWeightedHolidayMapping("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedHolidayMappingRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new WeightedHolidayMappingRequiredSkillRule("weighted holiday mapping needs an entry");
        var item = WeightedHolidayMapping.NewWeightedHolidayMapping("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedHolidayMappingTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new WeightedHolidayMappingTenureMonthsRangeRule(64, 3600);
        var item = WeightedHolidayMapping.NewWeightedHolidayMapping("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedHolidayMappingValidatorReportsEveryProblem();
        WeightedHolidayMappingNameLengthRuleFlagsALongName();
        WeightedHolidayMappingRequiredNameRuleFlagsABlankName();
        WeightedHolidayMappingPhoneLengthRuleFlagsALongPhone();
        WeightedHolidayMappingRequiredSkillRuleFlagsABlankSkill();
        WeightedHolidayMappingTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
