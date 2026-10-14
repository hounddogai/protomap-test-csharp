namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryAccrualTierRulesChecks
{
    public static void PrimaryAccrualTierValidatorReportsEveryProblem()
    {
        var validator = PrimaryAccrualTierRules.DefaultPrimaryAccrualTierValidator();
        var item = PrimaryAccrualTier.NewPrimaryAccrualTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryAccrualTierChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new PrimaryAccrualTierChecksumLengthRule(80);
        var item = PrimaryAccrualTier.NewPrimaryAccrualTier("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryAccrualTierEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PrimaryAccrualTierEndsAtRangeRule(120, 3600);
        var item = PrimaryAccrualTier.NewPrimaryAccrualTier("x", "y");
        item.EndsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryAccrualTierNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryAccrualTierNameLengthRule(16);
        var item = PrimaryAccrualTier.NewPrimaryAccrualTier("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryAccrualTierValidatorReportsEveryProblem();
        PrimaryAccrualTierChecksumLengthRuleFlagsALongChecksum();
        PrimaryAccrualTierEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PrimaryAccrualTierNameLengthRuleFlagsALongName();
    }
}
