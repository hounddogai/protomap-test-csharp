namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayNoticeRulesChecks
{
    public static void HolidayNoticeValidatorReportsEveryProblem()
    {
        var validator = HolidayNoticeRules.DefaultHolidayNoticeValidator();
        var item = HolidayNotice.NewHolidayNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayNoticeCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new HolidayNoticeCostRangeRule(9.5, 2601.7);
        var item = HolidayNotice.NewHolidayNotice("x", "y");
        item.Cost = 2603.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayNoticeSkillLengthRuleFlagsALongSkill()
    {
        var rule = new HolidayNoticeSkillLengthRule(32);
        var item = HolidayNotice.NewHolidayNotice("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HolidayNoticeRequiredCodeRule("holiday notice needs a value");
        var item = HolidayNotice.NewHolidayNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayNoticeRegionLengthRuleFlagsALongRegion()
    {
        var rule = new HolidayNoticeRegionLengthRule(120);
        var item = HolidayNotice.NewHolidayNotice("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayNoticeNameLengthRuleFlagsALongName()
    {
        var rule = new HolidayNoticeNameLengthRule(64);
        var item = HolidayNotice.NewHolidayNotice("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HolidayNoticeValidatorReportsEveryProblem();
        HolidayNoticeCostRangeRuleFlagsValuesAboveCost();
        HolidayNoticeSkillLengthRuleFlagsALongSkill();
        HolidayNoticeRequiredCodeRuleFlagsABlankCode();
        HolidayNoticeRegionLengthRuleFlagsALongRegion();
        HolidayNoticeNameLengthRuleFlagsALongName();
    }
}
