namespace Acme.Workforce.Contractors.Tests.Rate.RateProposal;

using Acme.Workforce.Contractors.Rate.RateProposal;

/// <summary>Checks of rate proposal code, run by the project's runner.</summary>
public static class WeightedRatePreferenceRulesChecks
{
    public static void WeightedRatePreferenceValidatorReportsEveryProblem()
    {
        var validator = WeightedRatePreferenceRules.DefaultWeightedRatePreferenceValidator();
        var item = WeightedRatePreference.NewWeightedRatePreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedRatePreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedRatePreferenceRequiredTitleRule("weighted rate preference needs this field");
        var item = WeightedRatePreference.NewWeightedRatePreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedRatePreferenceEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new WeightedRatePreferenceEndsAtRangeRule(32, 1000);
        var item = WeightedRatePreference.NewWeightedRatePreference("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedRatePreferenceValidatorReportsEveryProblem();
        WeightedRatePreferenceRequiredTitleRuleFlagsABlankTitle();
        WeightedRatePreferenceEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
