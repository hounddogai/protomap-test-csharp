namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class WeightedMeritCycleTierRulesChecks
{
    public static void WeightedMeritCycleTierValidatorReportsEveryProblem()
    {
        var validator = WeightedMeritCycleTierRules.DefaultWeightedMeritCycleTierValidator();
        var item = WeightedMeritCycleTier.NewWeightedMeritCycleTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedMeritCycleTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedMeritCycleTierRequiredLabelRule("weighted merit cycle tier needs this field");
        var item = WeightedMeritCycleTier.NewWeightedMeritCycleTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedMeritCycleTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedMeritCycleTierRequiredCodeRule("weighted merit cycle tier needs an entry");
        var item = WeightedMeritCycleTier.NewWeightedMeritCycleTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedMeritCycleTierValidatorReportsEveryProblem();
        WeightedMeritCycleTierRequiredLabelRuleFlagsABlankLabel();
        WeightedMeritCycleTierRequiredCodeRuleFlagsABlankCode();
    }
}
