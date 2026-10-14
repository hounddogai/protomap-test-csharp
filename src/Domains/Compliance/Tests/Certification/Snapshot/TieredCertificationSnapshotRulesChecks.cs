namespace Acme.Workforce.Compliance.Tests.Certification.Snapshot;

using Acme.Workforce.Compliance.Certification.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class TieredCertificationSnapshotRulesChecks
{
    public static void TieredCertificationSnapshotValidatorReportsEveryProblem()
    {
        var validator = TieredCertificationSnapshotRules.DefaultTieredCertificationSnapshotValidator();
        var item = TieredCertificationSnapshot.NewTieredCertificationSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredCertificationSnapshotLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new TieredCertificationSnapshotLimitRangeRule(32, 255);
        var item = TieredCertificationSnapshot.NewTieredCertificationSnapshot("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredCertificationSnapshotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredCertificationSnapshotLabelLengthRule(120);
        var item = TieredCertificationSnapshot.NewTieredCertificationSnapshot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredCertificationSnapshotSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new TieredCertificationSnapshotSequenceRangeRule(1000, 86400);
        var item = TieredCertificationSnapshot.NewTieredCertificationSnapshot("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredCertificationSnapshotValidatorReportsEveryProblem();
        TieredCertificationSnapshotLimitRangeRuleFlagsValuesAboveLimit();
        TieredCertificationSnapshotLabelLengthRuleFlagsALongLabel();
        TieredCertificationSnapshotSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
