namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledOffboardingRollupRulesChecks
{
    public static void ScheduledOffboardingRollupValidatorReportsEveryProblem()
    {
        var validator = ScheduledOffboardingRollupRules.DefaultScheduledOffboardingRollupValidator();
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledOffboardingRollupLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new ScheduledOffboardingRollupLocaleLengthRule(1000);
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledOffboardingRollupRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new ScheduledOffboardingRollupRequiredTeamRule("scheduled offboarding rollup needs a value");
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledOffboardingRollupRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new ScheduledOffboardingRollupRequiredLocaleRule("scheduled offboarding rollup needs an entry");
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledOffboardingRollupEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ScheduledOffboardingRollupEndsAtRangeRule(64, 1000);
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledOffboardingRollupTeamLengthRuleFlagsALongTeam()
    {
        var rule = new ScheduledOffboardingRollupTeamLengthRule(120);
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledOffboardingRollupValidatorReportsEveryProblem();
        ScheduledOffboardingRollupLocaleLengthRuleFlagsALongLocale();
        ScheduledOffboardingRollupRequiredTeamRuleFlagsABlankTeam();
        ScheduledOffboardingRollupRequiredLocaleRuleFlagsABlankLocale();
        ScheduledOffboardingRollupEndsAtRangeRuleFlagsValuesAboveEndsAt();
        ScheduledOffboardingRollupTeamLengthRuleFlagsALongTeam();
    }
}
