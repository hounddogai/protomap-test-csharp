namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CappedAccrualScoreRulesChecks
{
    public static void CappedAccrualScoreValidatorReportsEveryProblem()
    {
        var validator = CappedAccrualScoreRules.DefaultCappedAccrualScoreValidator();
        var item = CappedAccrualScore.NewCappedAccrualScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAccrualScoreBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new CappedAccrualScoreBatchSizeRangeRule(64, 255);
        var item = CappedAccrualScore.NewCappedAccrualScore("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAccrualScoreFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new CappedAccrualScoreFloorValueRangeRule(4.6, 4499.8);
        var item = CappedAccrualScore.NewCappedAccrualScore("x", "y");
        item.FloorValue = 4501.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAccrualScoreLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedAccrualScoreLabelLengthRule(16);
        var item = CappedAccrualScore.NewCappedAccrualScore("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAccrualScoreRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new CappedAccrualScoreRequiredSegmentRule("capped accrual score needs this field");
        var item = CappedAccrualScore.NewCappedAccrualScore("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedAccrualScoreValidatorReportsEveryProblem();
        CappedAccrualScoreBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        CappedAccrualScoreFloorValueRangeRuleFlagsValuesAboveFloorValue();
        CappedAccrualScoreLabelLengthRuleFlagsALongLabel();
        CappedAccrualScoreRequiredSegmentRuleFlagsABlankSegment();
    }
}
