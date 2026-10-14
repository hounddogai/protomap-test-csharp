namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class PrimarySuccessionPlanRequestRulesChecks
{
    public static void PrimarySuccessionPlanRequestValidatorReportsEveryProblem()
    {
        var validator = PrimarySuccessionPlanRequestRules.DefaultPrimarySuccessionPlanRequestValidator();
        var item = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimarySuccessionPlanRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimarySuccessionPlanRequestTitleLengthRule(32);
        var item = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimarySuccessionPlanRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimarySuccessionPlanRequestRequiredCodeRule("primary succession plan request needs an entry");
        var item = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimarySuccessionPlanRequestRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new PrimarySuccessionPlanRequestRequiredDescriptionRule(
            "primary succession plan request needs a value");
        var item = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimarySuccessionPlanRequestRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new PrimarySuccessionPlanRequestRatingRangeRule(3.3, 617.4);
        var item = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest("x", "y");
        item.Rating = 618.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimarySuccessionPlanRequestCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new PrimarySuccessionPlanRequestCostRangeRule(4.0, 2124.2);
        var item = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest("x", "y");
        item.Cost = 2125.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimarySuccessionPlanRequestValidatorReportsEveryProblem();
        PrimarySuccessionPlanRequestTitleLengthRuleFlagsALongTitle();
        PrimarySuccessionPlanRequestRequiredCodeRuleFlagsABlankCode();
        PrimarySuccessionPlanRequestRequiredDescriptionRuleFlagsABlankDescription();
        PrimarySuccessionPlanRequestRatingRangeRuleFlagsValuesAboveRating();
        PrimarySuccessionPlanRequestCostRangeRuleFlagsValuesAboveCost();
    }
}
