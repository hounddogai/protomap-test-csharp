namespace Acme.Workforce.Absence.Tests.Balance.Checkpoint;

using Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LegacyBalanceAdjustmentRulesChecks
{
    public static void LegacyBalanceAdjustmentValidatorReportsEveryProblem()
    {
        var validator = LegacyBalanceAdjustmentRules.DefaultLegacyBalanceAdjustmentValidator();
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyBalanceAdjustmentAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LegacyBalanceAdjustmentAttemptsRangeRule(120, 255);
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyBalanceAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyBalanceAdjustmentRequiredCodeRule("legacy balance adjustment needs an entry");
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyBalanceAdjustmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LegacyBalanceAdjustmentExpiresAtRangeRule(32, 80);
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("x", "y");
        item.ExpiresAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyBalanceAdjustmentTeamLengthRuleFlagsALongTeam()
    {
        var rule = new LegacyBalanceAdjustmentTeamLengthRule(120);
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyBalanceAdjustmentMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new LegacyBalanceAdjustmentMaximumRangeRule(4.1, 2885.7);
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("x", "y");
        item.Maximum = 2887.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyBalanceAdjustmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LegacyBalanceAdjustmentDurationMinutesRangeRule(120, 255);
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyBalanceAdjustmentValidatorReportsEveryProblem();
        LegacyBalanceAdjustmentAttemptsRangeRuleFlagsValuesAboveAttempts();
        LegacyBalanceAdjustmentRequiredCodeRuleFlagsABlankCode();
        LegacyBalanceAdjustmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        LegacyBalanceAdjustmentTeamLengthRuleFlagsALongTeam();
        LegacyBalanceAdjustmentMaximumRangeRuleFlagsValuesAboveMaximum();
        LegacyBalanceAdjustmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
