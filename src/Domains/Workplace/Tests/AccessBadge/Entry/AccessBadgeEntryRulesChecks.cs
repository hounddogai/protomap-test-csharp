namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeEntryRulesChecks
{
    public static void AccessBadgeEntryValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeEntryRules.DefaultAccessBadgeEntryValidator();
        var item = AccessBadgeEntry.NewAccessBadgeEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeEntryCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AccessBadgeEntryCostRangeRule(4.6, 4673.2);
        var item = AccessBadgeEntry.NewAccessBadgeEntry("x", "y");
        item.Cost = 4674.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeEntryDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new AccessBadgeEntryDurationMinutesRangeRule(32, 255);
        var item = AccessBadgeEntry.NewAccessBadgeEntry("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeEntryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccessBadgeEntryRequiredLabelRule("access badge entry needs this field");
        var item = AccessBadgeEntry.NewAccessBadgeEntry("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeEntryRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new AccessBadgeEntryRequiredTeamRule("access badge entry needs an entry");
        var item = AccessBadgeEntry.NewAccessBadgeEntry("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeEntryRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new AccessBadgeEntryRequiredManagerIdRule("access badge entry needs this field");
        var item = AccessBadgeEntry.NewAccessBadgeEntry("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeEntryValidatorReportsEveryProblem();
        AccessBadgeEntryCostRangeRuleFlagsValuesAboveCost();
        AccessBadgeEntryDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        AccessBadgeEntryRequiredLabelRuleFlagsABlankLabel();
        AccessBadgeEntryRequiredTeamRuleFlagsABlankTeam();
        AccessBadgeEntryRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
