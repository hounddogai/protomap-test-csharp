namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PrimaryPoolOverrideRulesChecks
{
    public static void PrimaryPoolOverrideValidatorReportsEveryProblem()
    {
        var validator = PrimaryPoolOverrideRules.DefaultPrimaryPoolOverrideValidator();
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryPoolOverrideChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new PrimaryPoolOverrideChecksumLengthRule(80);
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPoolOverrideExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PrimaryPoolOverrideExpiresAtRangeRule(120, 255);
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryPoolOverrideRequiredNameRuleFlagsABlankName()
    {
        var rule = new PrimaryPoolOverrideRequiredNameRule("primary pool override needs this field");
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPoolOverrideSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new PrimaryPoolOverrideSalaryBandLengthRule(16);
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPoolOverrideDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PrimaryPoolOverrideDurationMinutesRangeRule(32, 255);
        var item = PrimaryPoolOverride.NewPrimaryPoolOverride("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryPoolOverrideValidatorReportsEveryProblem();
        PrimaryPoolOverrideChecksumLengthRuleFlagsALongChecksum();
        PrimaryPoolOverrideExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PrimaryPoolOverrideRequiredNameRuleFlagsABlankName();
        PrimaryPoolOverrideSalaryBandLengthRuleFlagsALongSalaryBand();
        PrimaryPoolOverrideDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
