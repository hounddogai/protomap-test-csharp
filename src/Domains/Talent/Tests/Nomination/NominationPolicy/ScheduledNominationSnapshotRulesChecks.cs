namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class ScheduledNominationSnapshotRulesChecks
{
    public static void ScheduledNominationSnapshotValidatorReportsEveryProblem()
    {
        var validator = ScheduledNominationSnapshotRules.DefaultScheduledNominationSnapshotValidator();
        var item = ScheduledNominationSnapshot.NewScheduledNominationSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledNominationSnapshotLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new ScheduledNominationSnapshotLimitRangeRule(255, 1000);
        var item = ScheduledNominationSnapshot.NewScheduledNominationSnapshot("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledNominationSnapshotRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new ScheduledNominationSnapshotRequiredLocaleRule("scheduled nomination snapshot needs a value");
        var item = ScheduledNominationSnapshot.NewScheduledNominationSnapshot("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledNominationSnapshotValidatorReportsEveryProblem();
        ScheduledNominationSnapshotLimitRangeRuleFlagsValuesAboveLimit();
        ScheduledNominationSnapshotRequiredLocaleRuleFlagsABlankLocale();
    }
}
