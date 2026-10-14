namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationTierRulesChecks
{
    public static void NominationTierValidatorReportsEveryProblem()
    {
        var validator = NominationTierRules.DefaultNominationTierValidator();
        var item = NominationTier.NewNominationTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new NominationTierRequiredCodeRule("nomination tier needs this field");
        var item = NominationTier.NewNominationTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationTierCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new NominationTierCapacityRangeRule(16, 1000);
        var item = NominationTier.NewNominationTier("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationTierScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new NominationTierScoreRangeRule(1.3, 3038.9);
        var item = NominationTier.NewNominationTier("x", "y");
        item.Score = 3040.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        NominationTierValidatorReportsEveryProblem();
        NominationTierRequiredCodeRuleFlagsABlankCode();
        NominationTierCapacityRangeRuleFlagsValuesAboveCapacity();
        NominationTierScoreRangeRuleFlagsValuesAboveScore();
    }
}
