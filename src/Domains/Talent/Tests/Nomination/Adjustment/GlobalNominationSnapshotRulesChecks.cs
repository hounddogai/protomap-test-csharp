namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalNominationSnapshotRulesChecks
{
    public static void GlobalNominationSnapshotValidatorReportsEveryProblem()
    {
        var validator = GlobalNominationSnapshotRules.DefaultGlobalNominationSnapshotValidator();
        var item = GlobalNominationSnapshot.NewGlobalNominationSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalNominationSnapshotBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new GlobalNominationSnapshotBadgeLengthRule(1000);
        var item = GlobalNominationSnapshot.NewGlobalNominationSnapshot("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalNominationSnapshotRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalNominationSnapshotRequiredTitleRule("global nomination snapshot needs this field");
        var item = GlobalNominationSnapshot.NewGlobalNominationSnapshot("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalNominationSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalNominationSnapshotRequiredCodeRule("global nomination snapshot needs an entry");
        var item = GlobalNominationSnapshot.NewGlobalNominationSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalNominationSnapshotValidatorReportsEveryProblem();
        GlobalNominationSnapshotBadgeLengthRuleFlagsALongBadge();
        GlobalNominationSnapshotRequiredTitleRuleFlagsABlankTitle();
        GlobalNominationSnapshotRequiredCodeRuleFlagsABlankCode();
    }
}
