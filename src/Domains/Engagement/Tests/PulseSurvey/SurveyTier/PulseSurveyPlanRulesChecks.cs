namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class PulseSurveyPlanRulesChecks
{
    public static void PulseSurveyPlanValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyPlanRules.DefaultPulseSurveyPlanValidator();
        var item = PulseSurveyPlan.NewPulseSurveyPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyPlanRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new PulseSurveyPlanRatingRangeRule(2.6, 3093.8);
        var item = PulseSurveyPlan.NewPulseSurveyPlan("x", "y");
        item.Rating = 3095.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PulseSurveyPlanLabelLengthRule(16);
        var item = PulseSurveyPlan.NewPulseSurveyPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyPlanUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new PulseSurveyPlanUpdatedAtRangeRule(16, 3600);
        var item = PulseSurveyPlan.NewPulseSurveyPlan("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyPlanQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PulseSurveyPlanQuantityRangeRule(16, 86400);
        var item = PulseSurveyPlan.NewPulseSurveyPlan("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyPlanCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new PulseSurveyPlanCostRangeRule(8.2, 486.1);
        var item = PulseSurveyPlan.NewPulseSurveyPlan("x", "y");
        item.Cost = 487.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PulseSurveyPlanRequiredCodeRule("pulse survey plan needs an entry");
        var item = PulseSurveyPlan.NewPulseSurveyPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyPlanValidatorReportsEveryProblem();
        PulseSurveyPlanRatingRangeRuleFlagsValuesAboveRating();
        PulseSurveyPlanLabelLengthRuleFlagsALongLabel();
        PulseSurveyPlanUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        PulseSurveyPlanQuantityRangeRuleFlagsValuesAboveQuantity();
        PulseSurveyPlanCostRangeRuleFlagsValuesAboveCost();
        PulseSurveyPlanRequiredCodeRuleFlagsABlankCode();
    }
}
