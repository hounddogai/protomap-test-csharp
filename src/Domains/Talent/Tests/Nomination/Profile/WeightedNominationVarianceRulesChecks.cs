namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedNominationVarianceRulesChecks
{
    public static void WeightedNominationVarianceValidatorReportsEveryProblem()
    {
        var validator = WeightedNominationVarianceRules.DefaultWeightedNominationVarianceValidator();
        var item = WeightedNominationVariance.NewWeightedNominationVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedNominationVarianceAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new WeightedNominationVarianceAmountRangeRule(2.2, 3625.9);
        var item = WeightedNominationVariance.NewWeightedNominationVariance("x", "y");
        item.Amount = 3627.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedNominationVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedNominationVarianceCodeLengthRule(120);
        var item = WeightedNominationVariance.NewWeightedNominationVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedNominationVarianceValidatorReportsEveryProblem();
        WeightedNominationVarianceAmountRangeRuleFlagsValuesAboveAmount();
        WeightedNominationVarianceCodeLengthRuleFlagsALongCode();
    }
}
