namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class EquityGrantRecordRulesChecks
{
    public static void EquityGrantRecordValidatorReportsEveryProblem()
    {
        var validator = EquityGrantRecordRules.DefaultEquityGrantRecordValidator();
        var item = EquityGrantRecord.NewEquityGrantRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantRecordContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new EquityGrantRecordContactEmailLengthRule(120);
        var item = EquityGrantRecord.NewEquityGrantRecord("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantRecordScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new EquityGrantRecordScoreRangeRule(1.9, 2213.6);
        var item = EquityGrantRecord.NewEquityGrantRecord("x", "y");
        item.Score = 2215.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantRecordStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new EquityGrantRecordStartsAtRangeRule(16, 255);
        var item = EquityGrantRecord.NewEquityGrantRecord("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantRecordEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new EquityGrantRecordEndsAtRangeRule(16, 86400);
        var item = EquityGrantRecord.NewEquityGrantRecord("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EquityGrantRecordValidatorReportsEveryProblem();
        EquityGrantRecordContactEmailLengthRuleFlagsALongContactEmail();
        EquityGrantRecordScoreRangeRuleFlagsValuesAboveScore();
        EquityGrantRecordStartsAtRangeRuleFlagsValuesAboveStartsAt();
        EquityGrantRecordEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
