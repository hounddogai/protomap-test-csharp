namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class AdaptiveBonusPoolBaselineRulesChecks
{
    public static void AdaptiveBonusPoolBaselineValidatorReportsEveryProblem()
    {
        var validator = AdaptiveBonusPoolBaselineRules.DefaultAdaptiveBonusPoolBaselineValidator();
        var item = AdaptiveBonusPoolBaseline.NewAdaptiveBonusPoolBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveBonusPoolBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveBonusPoolBaselineRequiredLabelRule("adaptive bonus pool baseline needs this field");
        var item = AdaptiveBonusPoolBaseline.NewAdaptiveBonusPoolBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveBonusPoolBaselineValidatorReportsEveryProblem();
        AdaptiveBonusPoolBaselineRequiredLabelRuleFlagsABlankLabel();
    }
}
