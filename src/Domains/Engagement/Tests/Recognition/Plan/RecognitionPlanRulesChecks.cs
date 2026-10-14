namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RecognitionPlanRulesChecks
{
    public static void RecognitionPlanValidatorReportsEveryProblem()
    {
        var validator = RecognitionPlanRules.DefaultRecognitionPlanValidator();
        var item = RecognitionPlan.NewRecognitionPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionPlanQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new RecognitionPlanQuantityRangeRule(16, 3600);
        var item = RecognitionPlan.NewRecognitionPlan("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionPlanRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new RecognitionPlanRequiredRegionRule("recognition plan needs a value");
        var item = RecognitionPlan.NewRecognitionPlan("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionPlanValidatorReportsEveryProblem();
        RecognitionPlanQuantityRangeRuleFlagsValuesAboveQuantity();
        RecognitionPlanRequiredRegionRuleFlagsABlankRegion();
    }
}
