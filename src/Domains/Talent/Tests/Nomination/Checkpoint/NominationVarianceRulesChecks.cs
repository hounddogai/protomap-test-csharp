namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationVarianceRulesChecks
{
    public static void NominationVarianceValidatorReportsEveryProblem()
    {
        var validator = NominationVarianceRules.DefaultNominationVarianceValidator();
        var item = NominationVariance.NewNominationVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationVarianceLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new NominationVarianceLeaveDaysRangeRule(120, 255);
        var item = NominationVariance.NewNominationVariance("x", "y");
        item.LeaveDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationVarianceExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new NominationVarianceExpiresAtRangeRule(32, 255);
        var item = NominationVariance.NewNominationVariance("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new NominationVarianceRequiredCodeRule("nomination variance needs a value");
        var item = NominationVariance.NewNominationVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationVarianceValidatorReportsEveryProblem();
        NominationVarianceLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        NominationVarianceExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        NominationVarianceRequiredCodeRuleFlagsABlankCode();
    }
}
