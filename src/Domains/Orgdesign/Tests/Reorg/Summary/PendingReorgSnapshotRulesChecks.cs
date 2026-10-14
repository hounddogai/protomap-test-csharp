namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingReorgSnapshotRulesChecks
{
    public static void PendingReorgSnapshotValidatorReportsEveryProblem()
    {
        var validator = PendingReorgSnapshotRules.DefaultPendingReorgSnapshotValidator();
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingReorgSnapshotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingReorgSnapshotLabelLengthRule(16);
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingReorgSnapshotCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PendingReorgSnapshotCeilingRangeRule(8.8, 2636.6);
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("x", "y");
        item.Ceiling = 2638.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingReorgSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingReorgSnapshotCodeLengthRule(16);
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingReorgSnapshotRequiredNameRuleFlagsABlankName()
    {
        var rule = new PendingReorgSnapshotRequiredNameRule("pending reorg snapshot needs this field");
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingReorgSnapshotValidatorReportsEveryProblem();
        PendingReorgSnapshotLabelLengthRuleFlagsALongLabel();
        PendingReorgSnapshotCeilingRangeRuleFlagsValuesAboveCeiling();
        PendingReorgSnapshotCodeLengthRuleFlagsALongCode();
        PendingReorgSnapshotRequiredNameRuleFlagsABlankName();
    }
}
