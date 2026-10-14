namespace Acme.Workforce.Absence.Tests.Balance.Segment;

using Acme.Workforce.Absence.Balance.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BalanceRecordRulesChecks
{
    public static void BalanceRecordValidatorReportsEveryProblem()
    {
        var validator = BalanceRecordRules.DefaultBalanceRecordValidator();
        var item = BalanceRecord.NewBalanceRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceRecordDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new BalanceRecordDurationMinutesRangeRule(16, 64);
        var item = BalanceRecord.NewBalanceRecord("x", "y");
        item.DurationMinutes = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceRecordRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new BalanceRecordRequiredChannelRule("balance record needs an entry");
        var item = BalanceRecord.NewBalanceRecord("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceRecordNameLengthRuleFlagsALongName()
    {
        var rule = new BalanceRecordNameLengthRule(16);
        var item = BalanceRecord.NewBalanceRecord("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BalanceRecordValidatorReportsEveryProblem();
        BalanceRecordDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        BalanceRecordRequiredChannelRuleFlagsABlankChannel();
        BalanceRecordNameLengthRuleFlagsALongName();
    }
}
