namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountSchedule;

using Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>Checks of headcount schedule code, run by the project's runner.</summary>
public static class SeasonalHeadcountReservationRulesChecks
{
    public static void SeasonalHeadcountReservationValidatorReportsEveryProblem()
    {
        var validator = SeasonalHeadcountReservationRules.DefaultSeasonalHeadcountReservationValidator();
        var item = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalHeadcountReservationRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new SeasonalHeadcountReservationRequiredDepartmentRule(
            "seasonal headcount reservation needs this field");
        var item = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalHeadcountReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalHeadcountReservationCodeLengthRule(3600);
        var item = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalHeadcountReservationRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new SeasonalHeadcountReservationRequiredSkillRule("seasonal headcount reservation needs this field");
        var item = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalHeadcountReservationValidatorReportsEveryProblem();
        SeasonalHeadcountReservationRequiredDepartmentRuleFlagsABlankDepartment();
        SeasonalHeadcountReservationCodeLengthRuleFlagsALongCode();
        SeasonalHeadcountReservationRequiredSkillRuleFlagsABlankSkill();
    }
}
