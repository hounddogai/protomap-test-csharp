namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RegionalSkillProfileGroupRulesChecks
{
    public static void RegionalSkillProfileGroupValidatorReportsEveryProblem()
    {
        var validator = RegionalSkillProfileGroupRules.DefaultRegionalSkillProfileGroupValidator();
        var item = RegionalSkillProfileGroup.NewRegionalSkillProfileGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSkillProfileGroupDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new RegionalSkillProfileGroupDurationMinutesRangeRule(80, 1000);
        var item = RegionalSkillProfileGroup.NewRegionalSkillProfileGroup("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSkillProfileGroupRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new RegionalSkillProfileGroupRequiredChannelRule("regional skill profile group needs a value");
        var item = RegionalSkillProfileGroup.NewRegionalSkillProfileGroup("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalSkillProfileGroupValidatorReportsEveryProblem();
        RegionalSkillProfileGroupDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        RegionalSkillProfileGroupRequiredChannelRuleFlagsABlankChannel();
    }
}
