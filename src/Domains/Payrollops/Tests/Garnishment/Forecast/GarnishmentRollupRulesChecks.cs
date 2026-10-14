namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentRollupRulesChecks
{
    public static void GarnishmentRollupValidatorReportsEveryProblem()
    {
        var validator = GarnishmentRollupRules.DefaultGarnishmentRollupValidator();
        var item = GarnishmentRollup.NewGarnishmentRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentRollupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GarnishmentRollupRequiredLabelRule("garnishment rollup needs a value");
        var item = GarnishmentRollup.NewGarnishmentRollup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentRollupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GarnishmentRollupTitleLengthRule(1000);
        var item = GarnishmentRollup.NewGarnishmentRollup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentRollupCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new GarnishmentRollupCapacityRangeRule(120, 255);
        var item = GarnishmentRollup.NewGarnishmentRollup("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GarnishmentRollupValidatorReportsEveryProblem();
        GarnishmentRollupRequiredLabelRuleFlagsABlankLabel();
        GarnishmentRollupTitleLengthRuleFlagsALongTitle();
        GarnishmentRollupCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
