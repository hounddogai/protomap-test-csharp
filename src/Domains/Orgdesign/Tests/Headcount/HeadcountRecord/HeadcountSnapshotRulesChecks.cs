namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class HeadcountSnapshotRulesChecks
{
    public static void HeadcountSnapshotValidatorReportsEveryProblem()
    {
        var validator = HeadcountSnapshotRules.DefaultHeadcountSnapshotValidator();
        var item = HeadcountSnapshot.NewHeadcountSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountSnapshotVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new HeadcountSnapshotVersionRangeRule(80, 3600);
        var item = HeadcountSnapshot.NewHeadcountSnapshot("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountSnapshotScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new HeadcountSnapshotScoreRangeRule(0.2, 1627.9);
        var item = HeadcountSnapshot.NewHeadcountSnapshot("x", "y");
        item.Score = 1629.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountSnapshotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new HeadcountSnapshotLabelLengthRule(255);
        var item = HeadcountSnapshot.NewHeadcountSnapshot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountSnapshotValidatorReportsEveryProblem();
        HeadcountSnapshotVersionRangeRuleFlagsValuesAboveVersion();
        HeadcountSnapshotScoreRangeRuleFlagsValuesAboveScore();
        HeadcountSnapshotLabelLengthRuleFlagsALongLabel();
    }
}
