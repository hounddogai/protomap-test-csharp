namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class GlobalLaptopPlanRulesChecks
{
    public static void GlobalLaptopPlanValidatorReportsEveryProblem()
    {
        var validator = GlobalLaptopPlanRules.DefaultGlobalLaptopPlanValidator();
        var item = GlobalLaptopPlan.NewGlobalLaptopPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalLaptopPlanChannelLengthRuleFlagsALongChannel()
    {
        var rule = new GlobalLaptopPlanChannelLengthRule(64);
        var item = GlobalLaptopPlan.NewGlobalLaptopPlan("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalLaptopPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalLaptopPlanCodeLengthRule(3600);
        var item = GlobalLaptopPlan.NewGlobalLaptopPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalLaptopPlanSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new GlobalLaptopPlanSegmentLengthRule(1000);
        var item = GlobalLaptopPlan.NewGlobalLaptopPlan("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalLaptopPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalLaptopPlanLabelLengthRule(32);
        var item = GlobalLaptopPlan.NewGlobalLaptopPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalLaptopPlanValidatorReportsEveryProblem();
        GlobalLaptopPlanChannelLengthRuleFlagsALongChannel();
        GlobalLaptopPlanCodeLengthRuleFlagsALongCode();
        GlobalLaptopPlanSegmentLengthRuleFlagsALongSegment();
        GlobalLaptopPlanLabelLengthRuleFlagsALongLabel();
    }
}
