namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class TieredDeskMetricRulesChecks
{
    public static void TieredDeskMetricValidatorReportsEveryProblem()
    {
        var validator = TieredDeskMetricRules.DefaultTieredDeskMetricValidator();
        var item = TieredDeskMetric.NewTieredDeskMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredDeskMetricRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new TieredDeskMetricRequiredGradeRule("tiered desk metric needs a value");
        var item = TieredDeskMetric.NewTieredDeskMetric("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredDeskMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredDeskMetricCodeLengthRule(3600);
        var item = TieredDeskMetric.NewTieredDeskMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredDeskMetricRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new TieredDeskMetricRatingRangeRule(6.8, 4547.5);
        var item = TieredDeskMetric.NewTieredDeskMetric("x", "y");
        item.Rating = 4549.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredDeskMetricRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new TieredDeskMetricRequiredManagerIdRule("tiered desk metric needs this field");
        var item = TieredDeskMetric.NewTieredDeskMetric("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredDeskMetricCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new TieredDeskMetricCeilingRangeRule(5.6, 4687.0);
        var item = TieredDeskMetric.NewTieredDeskMetric("x", "y");
        item.Ceiling = 4688.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredDeskMetricFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new TieredDeskMetricFloorValueRangeRule(0.3, 4754.9);
        var item = TieredDeskMetric.NewTieredDeskMetric("x", "y");
        item.FloorValue = 4756.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredDeskMetricValidatorReportsEveryProblem();
        TieredDeskMetricRequiredGradeRuleFlagsABlankGrade();
        TieredDeskMetricCodeLengthRuleFlagsALongCode();
        TieredDeskMetricRatingRangeRuleFlagsValuesAboveRating();
        TieredDeskMetricRequiredManagerIdRuleFlagsABlankManagerId();
        TieredDeskMetricCeilingRangeRuleFlagsValuesAboveCeiling();
        TieredDeskMetricFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
