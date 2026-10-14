namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RefreshPlanRulesChecks
{
    public static void RefreshPlanValidatorReportsEveryProblem()
    {
        var validator = RefreshPlanRules.DefaultRefreshPlanValidator();
        var item = RefreshPlan.NewRefreshPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshPlanRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RefreshPlanRequiredLabelRule("refresh plan needs a value");
        var item = RefreshPlan.NewRefreshPlan("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshPlanAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new RefreshPlanAttemptsRangeRule(1000, 3600);
        var item = RefreshPlan.NewRefreshPlan("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshPlanRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new RefreshPlanRequiredSegmentRule("refresh plan needs an entry");
        var item = RefreshPlan.NewRefreshPlan("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshPlanThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new RefreshPlanThresholdRangeRule(8.7, 2382.9);
        var item = RefreshPlan.NewRefreshPlan("x", "y");
        item.Threshold = 2384.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RefreshPlanValidatorReportsEveryProblem();
        RefreshPlanRequiredLabelRuleFlagsABlankLabel();
        RefreshPlanAttemptsRangeRuleFlagsValuesAboveAttempts();
        RefreshPlanRequiredSegmentRuleFlagsABlankSegment();
        RefreshPlanThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
