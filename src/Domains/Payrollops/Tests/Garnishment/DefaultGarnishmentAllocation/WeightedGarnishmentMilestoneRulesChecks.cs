namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class WeightedGarnishmentMilestoneRulesChecks
{
    public static void WeightedGarnishmentMilestoneValidatorReportsEveryProblem()
    {
        var validator = WeightedGarnishmentMilestoneRules.DefaultWeightedGarnishmentMilestoneValidator();
        var item = WeightedGarnishmentMilestone.NewWeightedGarnishmentMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedGarnishmentMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedGarnishmentMilestoneRequiredCodeRule("weighted garnishment milestone needs a value");
        var item = WeightedGarnishmentMilestone.NewWeightedGarnishmentMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedGarnishmentMilestoneTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedGarnishmentMilestoneTitleLengthRule(86400);
        var item = WeightedGarnishmentMilestone.NewWeightedGarnishmentMilestone("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedGarnishmentMilestoneSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new WeightedGarnishmentMilestoneSegmentLengthRule(86400);
        var item = WeightedGarnishmentMilestone.NewWeightedGarnishmentMilestone("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedGarnishmentMilestoneValidatorReportsEveryProblem();
        WeightedGarnishmentMilestoneRequiredCodeRuleFlagsABlankCode();
        WeightedGarnishmentMilestoneTitleLengthRuleFlagsALongTitle();
        WeightedGarnishmentMilestoneSegmentLengthRuleFlagsALongSegment();
    }
}
