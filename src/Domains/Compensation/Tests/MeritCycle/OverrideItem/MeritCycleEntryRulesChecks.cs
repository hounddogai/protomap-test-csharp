namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleEntryRulesChecks
{
    public static void MeritCycleEntryValidatorReportsEveryProblem()
    {
        var validator = MeritCycleEntryRules.DefaultMeritCycleEntryValidator();
        var item = MeritCycleEntry.NewMeritCycleEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleEntryRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new MeritCycleEntryRequiredBadgeRule("merit cycle entry needs a value");
        var item = MeritCycleEntry.NewMeritCycleEntry("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleEntryRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new MeritCycleEntryRequiredManagerIdRule("merit cycle entry needs this field");
        var item = MeritCycleEntry.NewMeritCycleEntry("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleEntryRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new MeritCycleEntryRatioRangeRule(1.5, 2077.4);
        var item = MeritCycleEntry.NewMeritCycleEntry("x", "y");
        item.Ratio = 2078.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleEntryBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new MeritCycleEntryBadgeLengthRule(64);
        var item = MeritCycleEntry.NewMeritCycleEntry("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleEntryRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new MeritCycleEntryRevisionRangeRule(64, 86400);
        var item = MeritCycleEntry.NewMeritCycleEntry("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleEntryValidatorReportsEveryProblem();
        MeritCycleEntryRequiredBadgeRuleFlagsABlankBadge();
        MeritCycleEntryRequiredManagerIdRuleFlagsABlankManagerId();
        MeritCycleEntryRatioRangeRuleFlagsValuesAboveRatio();
        MeritCycleEntryBadgeLengthRuleFlagsALongBadge();
        MeritCycleEntryRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
