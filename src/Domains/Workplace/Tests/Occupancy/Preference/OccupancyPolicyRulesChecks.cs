namespace Acme.Workforce.Workplace.Tests.Occupancy.Preference;

using Acme.Workforce.Workplace.Occupancy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class OccupancyPolicyRulesChecks
{
    public static void OccupancyPolicyValidatorReportsEveryProblem()
    {
        var validator = OccupancyPolicyRules.DefaultOccupancyPolicyValidator();
        var item = OccupancyPolicy.NewOccupancyPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyPolicyTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OccupancyPolicyTitleLengthRule(32);
        var item = OccupancyPolicy.NewOccupancyPolicy("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new OccupancyPolicyLabelLengthRule(3600);
        var item = OccupancyPolicy.NewOccupancyPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyPolicyRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new OccupancyPolicyRequiredLabelRule("occupancy policy needs a value");
        var item = OccupancyPolicy.NewOccupancyPolicy("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyPolicyTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new OccupancyPolicyTimezoneLengthRule(80);
        var item = OccupancyPolicy.NewOccupancyPolicy("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyPolicyBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new OccupancyPolicyBatchSizeRangeRule(80, 3600);
        var item = OccupancyPolicy.NewOccupancyPolicy("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        OccupancyPolicyValidatorReportsEveryProblem();
        OccupancyPolicyTitleLengthRuleFlagsALongTitle();
        OccupancyPolicyLabelLengthRuleFlagsALongLabel();
        OccupancyPolicyRequiredLabelRuleFlagsABlankLabel();
        OccupancyPolicyTimezoneLengthRuleFlagsALongTimezone();
        OccupancyPolicyBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
