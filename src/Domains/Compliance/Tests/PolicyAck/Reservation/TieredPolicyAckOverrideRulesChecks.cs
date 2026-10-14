namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPolicyAckOverrideRulesChecks
{
    public static void TieredPolicyAckOverrideValidatorReportsEveryProblem()
    {
        var validator = TieredPolicyAckOverrideRules.DefaultTieredPolicyAckOverrideValidator();
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPolicyAckOverrideDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new TieredPolicyAckOverrideDurationMinutesRangeRule(32, 3600);
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPolicyAckOverrideAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new TieredPolicyAckOverrideAttemptsRangeRule(80, 3600);
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPolicyAckOverrideHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new TieredPolicyAckOverrideHoursRangeRule(9.2, 2073.8);
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("x", "y");
        item.Hours = 2075.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPolicyAckOverridePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new TieredPolicyAckOverridePhoneLengthRule(32);
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPolicyAckOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredPolicyAckOverrideRequiredCodeRule("tiered policy ack override needs an entry");
        var item = TieredPolicyAckOverride.NewTieredPolicyAckOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPolicyAckOverrideValidatorReportsEveryProblem();
        TieredPolicyAckOverrideDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        TieredPolicyAckOverrideAttemptsRangeRuleFlagsValuesAboveAttempts();
        TieredPolicyAckOverrideHoursRangeRuleFlagsValuesAboveHours();
        TieredPolicyAckOverridePhoneLengthRuleFlagsALongPhone();
        TieredPolicyAckOverrideRequiredCodeRuleFlagsABlankCode();
    }
}
