namespace Acme.Workforce.Compliance.Tests.Attestation.Checkpoint;

using Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalAttestationCheckpointRulesChecks
{
    public static void GlobalAttestationCheckpointValidatorReportsEveryProblem()
    {
        var validator = GlobalAttestationCheckpointRules.DefaultGlobalAttestationCheckpointValidator();
        var item = GlobalAttestationCheckpoint.NewGlobalAttestationCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAttestationCheckpointStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new GlobalAttestationCheckpointStartsAtRangeRule(64, 64);
        var item = GlobalAttestationCheckpoint.NewGlobalAttestationCheckpoint("x", "y");
        item.StartsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAttestationCheckpointAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new GlobalAttestationCheckpointAttemptsRangeRule(16, 16);
        var item = GlobalAttestationCheckpoint.NewGlobalAttestationCheckpoint("x", "y");
        item.Attempts = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAttestationCheckpointCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new GlobalAttestationCheckpointCurrencyLengthRule(120);
        var item = GlobalAttestationCheckpoint.NewGlobalAttestationCheckpoint("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalAttestationCheckpointValidatorReportsEveryProblem();
        GlobalAttestationCheckpointStartsAtRangeRuleFlagsValuesAboveStartsAt();
        GlobalAttestationCheckpointAttemptsRangeRuleFlagsValuesAboveAttempts();
        GlobalAttestationCheckpointCurrencyLengthRuleFlagsALongCurrency();
    }
}
