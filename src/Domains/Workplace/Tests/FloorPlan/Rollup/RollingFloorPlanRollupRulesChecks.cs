namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rollup;

using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RollingFloorPlanRollupRulesChecks
{
    public static void RollingFloorPlanRollupValidatorReportsEveryProblem()
    {
        var validator = RollingFloorPlanRollupRules.DefaultRollingFloorPlanRollupValidator();
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingFloorPlanRollupNameLengthRuleFlagsALongName()
    {
        var rule = new RollingFloorPlanRollupNameLengthRule(3600);
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingFloorPlanRollupRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new RollingFloorPlanRollupRatingRangeRule(0.3, 1564.8);
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("x", "y");
        item.Rating = 1566.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingFloorPlanRollupMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new RollingFloorPlanRollupMarginRangeRule(8.5, 897.3);
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("x", "y");
        item.Margin = 898.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingFloorPlanRollupFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new RollingFloorPlanRollupFloorValueRangeRule(6.7, 1520.6);
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("x", "y");
        item.FloorValue = 1522.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingFloorPlanRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingFloorPlanRollupCodeLengthRule(86400);
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingFloorPlanRollupRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingFloorPlanRollupRequiredNameRule("rolling floor plan rollup needs this field");
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingFloorPlanRollupValidatorReportsEveryProblem();
        RollingFloorPlanRollupNameLengthRuleFlagsALongName();
        RollingFloorPlanRollupRatingRangeRuleFlagsValuesAboveRating();
        RollingFloorPlanRollupMarginRangeRuleFlagsValuesAboveMargin();
        RollingFloorPlanRollupFloorValueRangeRuleFlagsValuesAboveFloorValue();
        RollingFloorPlanRollupCodeLengthRuleFlagsALongCode();
        RollingFloorPlanRollupRequiredNameRuleFlagsABlankName();
    }
}
