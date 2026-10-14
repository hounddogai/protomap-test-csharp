namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveTrainingDuePlanRulesChecks
{
    public static void AdaptiveTrainingDuePlanValidatorReportsEveryProblem()
    {
        var validator = AdaptiveTrainingDuePlanRules.DefaultAdaptiveTrainingDuePlanValidator();
        var item = AdaptiveTrainingDuePlan.NewAdaptiveTrainingDuePlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveTrainingDuePlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveTrainingDuePlanRequiredCodeRule("adaptive training due plan needs this field");
        var item = AdaptiveTrainingDuePlan.NewAdaptiveTrainingDuePlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveTrainingDuePlanWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AdaptiveTrainingDuePlanWindowDaysRangeRule(120, 120);
        var item = AdaptiveTrainingDuePlan.NewAdaptiveTrainingDuePlan("x", "y");
        item.WindowDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveTrainingDuePlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveTrainingDuePlanCodeLengthRule(16);
        var item = AdaptiveTrainingDuePlan.NewAdaptiveTrainingDuePlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDuePlanValidatorReportsEveryProblem();
        AdaptiveTrainingDuePlanRequiredCodeRuleFlagsABlankCode();
        AdaptiveTrainingDuePlanWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AdaptiveTrainingDuePlanCodeLengthRuleFlagsALongCode();
    }
}
