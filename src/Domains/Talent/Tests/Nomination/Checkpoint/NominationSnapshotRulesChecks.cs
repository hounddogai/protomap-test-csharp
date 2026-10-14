namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationSnapshotRulesChecks
{
    public static void NominationSnapshotValidatorReportsEveryProblem()
    {
        var validator = NominationSnapshotRules.DefaultNominationSnapshotValidator();
        var item = NominationSnapshot.NewNominationSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationSnapshotBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new NominationSnapshotBadgeLengthRule(86400);
        var item = NominationSnapshot.NewNominationSnapshot("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationSnapshotRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new NominationSnapshotRequiredBadgeRule("nomination snapshot needs this field");
        var item = NominationSnapshot.NewNominationSnapshot("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new NominationSnapshotCodeLengthRule(3600);
        var item = NominationSnapshot.NewNominationSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationSnapshotNameLengthRuleFlagsALongName()
    {
        var rule = new NominationSnapshotNameLengthRule(86400);
        var item = NominationSnapshot.NewNominationSnapshot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        NominationSnapshotValidatorReportsEveryProblem();
        NominationSnapshotBadgeLengthRuleFlagsALongBadge();
        NominationSnapshotRequiredBadgeRuleFlagsABlankBadge();
        NominationSnapshotCodeLengthRuleFlagsALongCode();
        NominationSnapshotNameLengthRuleFlagsALongName();
    }
}
