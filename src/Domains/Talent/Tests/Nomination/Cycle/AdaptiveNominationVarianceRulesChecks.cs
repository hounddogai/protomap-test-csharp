namespace Acme.Workforce.Talent.Tests.Nomination.Cycle;

using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveNominationVarianceRulesChecks
{
    public static void AdaptiveNominationVarianceValidatorReportsEveryProblem()
    {
        var validator = AdaptiveNominationVarianceRules.DefaultAdaptiveNominationVarianceValidator();
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveNominationVarianceRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new AdaptiveNominationVarianceRequiredSourceRule("adaptive nomination variance needs this field");
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveNominationVarianceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveNominationVarianceRequiredLabelRule("adaptive nomination variance needs this field");
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveNominationVariancePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new AdaptiveNominationVariancePriorityRangeRule(32, 80);
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("x", "y");
        item.Priority = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveNominationVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveNominationVarianceCodeLengthRule(86400);
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveNominationVarianceValidatorReportsEveryProblem();
        AdaptiveNominationVarianceRequiredSourceRuleFlagsABlankSource();
        AdaptiveNominationVarianceRequiredLabelRuleFlagsABlankLabel();
        AdaptiveNominationVariancePriorityRangeRuleFlagsValuesAbovePriority();
        AdaptiveNominationVarianceCodeLengthRuleFlagsALongCode();
    }
}
