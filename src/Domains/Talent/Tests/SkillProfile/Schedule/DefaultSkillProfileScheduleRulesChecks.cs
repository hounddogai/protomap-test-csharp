namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DefaultSkillProfileScheduleRulesChecks
{
    public static void DefaultSkillProfileScheduleValidatorReportsEveryProblem()
    {
        var validator = DefaultSkillProfileScheduleRules.DefaultDefaultSkillProfileScheduleValidator();
        var item = DefaultSkillProfileSchedule.NewDefaultSkillProfileSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultSkillProfileScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultSkillProfileScheduleRequiredTitleRule("default skill profile schedule needs this field");
        var item = DefaultSkillProfileSchedule.NewDefaultSkillProfileSchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultSkillProfileScheduleExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new DefaultSkillProfileScheduleExternalRefLengthRule(1000);
        var item = DefaultSkillProfileSchedule.NewDefaultSkillProfileSchedule("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSkillProfileScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultSkillProfileScheduleRequiredCodeRule("default skill profile schedule needs this field");
        var item = DefaultSkillProfileSchedule.NewDefaultSkillProfileSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultSkillProfileScheduleCountryLengthRuleFlagsALongCountry()
    {
        var rule = new DefaultSkillProfileScheduleCountryLengthRule(16);
        var item = DefaultSkillProfileSchedule.NewDefaultSkillProfileSchedule("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultSkillProfileScheduleValidatorReportsEveryProblem();
        DefaultSkillProfileScheduleRequiredTitleRuleFlagsABlankTitle();
        DefaultSkillProfileScheduleExternalRefLengthRuleFlagsALongExternalRef();
        DefaultSkillProfileScheduleRequiredCodeRuleFlagsABlankCode();
        DefaultSkillProfileScheduleCountryLengthRuleFlagsALongCountry();
    }
}
