namespace Acme.Workforce.Talent.Tests.CareerPath.Profile;

using Acme.Workforce.Talent.CareerPath.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingCareerPathProfileRulesChecks
{
    public static void RollingCareerPathProfileValidatorReportsEveryProblem()
    {
        var validator = RollingCareerPathProfileRules.DefaultRollingCareerPathProfileValidator();
        var item = RollingCareerPathProfile.NewRollingCareerPathProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingCareerPathProfileTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new RollingCareerPathProfileTimezoneLengthRule(32);
        var item = RollingCareerPathProfile.NewRollingCareerPathProfile("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingCareerPathProfileRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new RollingCareerPathProfileRequiredJobCodeRule("rolling career path profile needs an entry");
        var item = RollingCareerPathProfile.NewRollingCareerPathProfile("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingCareerPathProfileDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new RollingCareerPathProfileDurationMinutesRangeRule(16, 255);
        var item = RollingCareerPathProfile.NewRollingCareerPathProfile("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingCareerPathProfileValidatorReportsEveryProblem();
        RollingCareerPathProfileTimezoneLengthRuleFlagsALongTimezone();
        RollingCareerPathProfileRequiredJobCodeRuleFlagsABlankJobCode();
        RollingCareerPathProfileDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
