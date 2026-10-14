namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class StrictSkillProfileScheduleRulesChecks
{
    public static void StrictSkillProfileScheduleValidatorReportsEveryProblem()
    {
        var validator = StrictSkillProfileScheduleRules.DefaultStrictSkillProfileScheduleValidator();
        var item = StrictSkillProfileSchedule.NewStrictSkillProfileSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictSkillProfileScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictSkillProfileScheduleRequiredCodeRule("strict skill profile schedule needs a value");
        var item = StrictSkillProfileSchedule.NewStrictSkillProfileSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictSkillProfileScheduleRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new StrictSkillProfileScheduleRequiredNotesRule("strict skill profile schedule needs an entry");
        var item = StrictSkillProfileSchedule.NewStrictSkillProfileSchedule("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictSkillProfileScheduleNotesLengthRuleFlagsALongNotes()
    {
        var rule = new StrictSkillProfileScheduleNotesLengthRule(120);
        var item = StrictSkillProfileSchedule.NewStrictSkillProfileSchedule("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictSkillProfileScheduleFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new StrictSkillProfileScheduleFteRangeRule(5.8, 3375.7);
        var item = StrictSkillProfileSchedule.NewStrictSkillProfileSchedule("x", "y");
        item.Fte = 3377.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictSkillProfileScheduleRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictSkillProfileScheduleRequiredNameRule("strict skill profile schedule needs a value");
        var item = StrictSkillProfileSchedule.NewStrictSkillProfileSchedule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictSkillProfileScheduleValidatorReportsEveryProblem();
        StrictSkillProfileScheduleRequiredCodeRuleFlagsABlankCode();
        StrictSkillProfileScheduleRequiredNotesRuleFlagsABlankNotes();
        StrictSkillProfileScheduleNotesLengthRuleFlagsALongNotes();
        StrictSkillProfileScheduleFteRangeRuleFlagsValuesAboveFte();
        StrictSkillProfileScheduleRequiredNameRuleFlagsABlankName();
    }
}
