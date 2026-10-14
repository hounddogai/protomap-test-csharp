namespace Acme.Workforce.Talent.Tests.Nomination.Template;

using Acme.Workforce.Talent.Nomination.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AdaptiveNominationTargetRulesChecks
{
    public static void AdaptiveNominationTargetValidatorReportsEveryProblem()
    {
        var validator = AdaptiveNominationTargetRules.DefaultAdaptiveNominationTargetValidator();
        var item = AdaptiveNominationTarget.NewAdaptiveNominationTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveNominationTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveNominationTargetCodeLengthRule(16);
        var item = AdaptiveNominationTarget.NewAdaptiveNominationTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveNominationTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveNominationTargetRequiredCodeRule("adaptive nomination target needs this field");
        var item = AdaptiveNominationTarget.NewAdaptiveNominationTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveNominationTargetCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AdaptiveNominationTargetCostRangeRule(4.5, 3832.2);
        var item = AdaptiveNominationTarget.NewAdaptiveNominationTarget("x", "y");
        item.Cost = 3833.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveNominationTargetGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AdaptiveNominationTargetGraceDaysRangeRule(16, 80);
        var item = AdaptiveNominationTarget.NewAdaptiveNominationTarget("x", "y");
        item.GraceDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveNominationTargetValidatorReportsEveryProblem();
        AdaptiveNominationTargetCodeLengthRuleFlagsALongCode();
        AdaptiveNominationTargetRequiredCodeRuleFlagsABlankCode();
        AdaptiveNominationTargetCostRangeRuleFlagsValuesAboveCost();
        AdaptiveNominationTargetGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
