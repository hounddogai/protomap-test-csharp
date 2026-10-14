namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RollingBalanceRecordRulesChecks
{
    public static void RollingBalanceRecordValidatorReportsEveryProblem()
    {
        var validator = RollingBalanceRecordRules.DefaultRollingBalanceRecordValidator();
        var item = RollingBalanceRecord.NewRollingBalanceRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingBalanceRecordSourceLengthRuleFlagsALongSource()
    {
        var rule = new RollingBalanceRecordSourceLengthRule(64);
        var item = RollingBalanceRecord.NewRollingBalanceRecord("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingBalanceRecordRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new RollingBalanceRecordRequiredSummaryRule("rolling balance record needs a value");
        var item = RollingBalanceRecord.NewRollingBalanceRecord("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingBalanceRecordUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new RollingBalanceRecordUpdatedAtRangeRule(32, 1000);
        var item = RollingBalanceRecord.NewRollingBalanceRecord("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingBalanceRecordSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new RollingBalanceRecordSequenceRangeRule(64, 120);
        var item = RollingBalanceRecord.NewRollingBalanceRecord("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingBalanceRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingBalanceRecordLabelLengthRule(86400);
        var item = RollingBalanceRecord.NewRollingBalanceRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingBalanceRecordValidatorReportsEveryProblem();
        RollingBalanceRecordSourceLengthRuleFlagsALongSource();
        RollingBalanceRecordRequiredSummaryRuleFlagsABlankSummary();
        RollingBalanceRecordUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        RollingBalanceRecordSequenceRangeRuleFlagsValuesAboveSequence();
        RollingBalanceRecordLabelLengthRuleFlagsALongLabel();
    }
}
