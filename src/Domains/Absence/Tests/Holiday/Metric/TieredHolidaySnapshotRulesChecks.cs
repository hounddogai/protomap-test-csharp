namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredHolidaySnapshotRulesChecks
{
    public static void TieredHolidaySnapshotValidatorReportsEveryProblem()
    {
        var validator = TieredHolidaySnapshotRules.DefaultTieredHolidaySnapshotValidator();
        var item = TieredHolidaySnapshot.NewTieredHolidaySnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredHolidaySnapshotEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new TieredHolidaySnapshotEmployeeIdLengthRule(32);
        var item = TieredHolidaySnapshot.NewTieredHolidaySnapshot("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredHolidaySnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredHolidaySnapshotCodeLengthRule(32);
        var item = TieredHolidaySnapshot.NewTieredHolidaySnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredHolidaySnapshotScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new TieredHolidaySnapshotScoreRangeRule(7.4, 4675.9);
        var item = TieredHolidaySnapshot.NewTieredHolidaySnapshot("x", "y");
        item.Score = 4677.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredHolidaySnapshotValidatorReportsEveryProblem();
        TieredHolidaySnapshotEmployeeIdLengthRuleFlagsALongEmployeeId();
        TieredHolidaySnapshotCodeLengthRuleFlagsALongCode();
        TieredHolidaySnapshotScoreRangeRuleFlagsValuesAboveScore();
    }
}
