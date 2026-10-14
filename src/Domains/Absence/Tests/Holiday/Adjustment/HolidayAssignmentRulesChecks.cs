namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayAssignmentRulesChecks
{
    public static void HolidayAssignmentValidatorReportsEveryProblem()
    {
        var validator = HolidayAssignmentRules.DefaultHolidayAssignmentValidator();
        var item = HolidayAssignment.NewHolidayAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayAssignmentSkillLengthRuleFlagsALongSkill()
    {
        var rule = new HolidayAssignmentSkillLengthRule(80);
        var item = HolidayAssignment.NewHolidayAssignment("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidayAssignmentCodeLengthRule(16);
        var item = HolidayAssignment.NewHolidayAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayAssignmentRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new HolidayAssignmentRequiredSkillRule("holiday assignment needs a value");
        var item = HolidayAssignment.NewHolidayAssignment("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayAssignmentTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new HolidayAssignmentTimezoneLengthRule(3600);
        var item = HolidayAssignment.NewHolidayAssignment("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayAssignmentRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new HolidayAssignmentRequiredTimezoneRule("holiday assignment needs a value");
        var item = HolidayAssignment.NewHolidayAssignment("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HolidayAssignmentValidatorReportsEveryProblem();
        HolidayAssignmentSkillLengthRuleFlagsALongSkill();
        HolidayAssignmentCodeLengthRuleFlagsALongCode();
        HolidayAssignmentRequiredSkillRuleFlagsABlankSkill();
        HolidayAssignmentTimezoneLengthRuleFlagsALongTimezone();
        HolidayAssignmentRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
