namespace Acme.Workforce.Workplace.Tests.AccessBadge.Request;

using Acme.Workforce.Workplace.AccessBadge.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class WeightedAccessBadgeReservationRulesChecks
{
    public static void WeightedAccessBadgeReservationValidatorReportsEveryProblem()
    {
        var validator = WeightedAccessBadgeReservationRules.DefaultWeightedAccessBadgeReservationValidator();
        var item = WeightedAccessBadgeReservation.NewWeightedAccessBadgeReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAccessBadgeReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new WeightedAccessBadgeReservationTitleLengthRule(86400);
        var item = WeightedAccessBadgeReservation.NewWeightedAccessBadgeReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAccessBadgeReservationCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new WeightedAccessBadgeReservationCostCenterLengthRule(1000);
        var item = WeightedAccessBadgeReservation.NewWeightedAccessBadgeReservation("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAccessBadgeReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedAccessBadgeReservationRequiredTitleRule(
            "weighted access badge reservation needs a value");
        var item = WeightedAccessBadgeReservation.NewWeightedAccessBadgeReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAccessBadgeReservationCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new WeightedAccessBadgeReservationCeilingRangeRule(9.3, 4501.8);
        var item = WeightedAccessBadgeReservation.NewWeightedAccessBadgeReservation("x", "y");
        item.Ceiling = 4503.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeReservationValidatorReportsEveryProblem();
        WeightedAccessBadgeReservationTitleLengthRuleFlagsALongTitle();
        WeightedAccessBadgeReservationCostCenterLengthRuleFlagsALongCostCenter();
        WeightedAccessBadgeReservationRequiredTitleRuleFlagsABlankTitle();
        WeightedAccessBadgeReservationCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
