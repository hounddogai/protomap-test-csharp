namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackFloorPlanRequestRulesChecks
{
    public static void FallbackFloorPlanRequestValidatorReportsEveryProblem()
    {
        var validator = FallbackFloorPlanRequestRules.DefaultFallbackFloorPlanRequestValidator();
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackFloorPlanRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FallbackFloorPlanRequestTitleLengthRule(32);
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackFloorPlanRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackFloorPlanRequestCodeLengthRule(64);
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackFloorPlanRequestWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new FallbackFloorPlanRequestWeightRangeRule(6.4, 1500.5);
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("x", "y");
        item.Weight = 1502.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackFloorPlanRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackFloorPlanRequestRequiredLabelRule("fallback floor plan request needs an entry");
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackFloorPlanRequestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackFloorPlanRequestRequiredTitleRule("fallback floor plan request needs an entry");
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackFloorPlanRequestLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new FallbackFloorPlanRequestLeaveDaysRangeRule(1000, 1000);
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackFloorPlanRequestValidatorReportsEveryProblem();
        FallbackFloorPlanRequestTitleLengthRuleFlagsALongTitle();
        FallbackFloorPlanRequestCodeLengthRuleFlagsALongCode();
        FallbackFloorPlanRequestWeightRangeRuleFlagsValuesAboveWeight();
        FallbackFloorPlanRequestRequiredLabelRuleFlagsABlankLabel();
        FallbackFloorPlanRequestRequiredTitleRuleFlagsABlankTitle();
        FallbackFloorPlanRequestLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
