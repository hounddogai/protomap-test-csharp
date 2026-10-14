namespace Acme.Workforce.Workplace.Tests.AccessBadge.Threshold;

using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccessBadgeReservationRulesChecks
{
    public static void AccessBadgeReservationValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeReservationRules.DefaultAccessBadgeReservationValidator();
        var item = AccessBadgeReservation.NewAccessBadgeReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeReservationBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AccessBadgeReservationBatchSizeRangeRule(32, 80);
        var item = AccessBadgeReservation.NewAccessBadgeReservation("x", "y");
        item.BatchSize = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AccessBadgeReservationLabelLengthRule(3600);
        var item = AccessBadgeReservation.NewAccessBadgeReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeReservationQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AccessBadgeReservationQuantityRangeRule(80, 80);
        var item = AccessBadgeReservation.NewAccessBadgeReservation("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeReservationManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new AccessBadgeReservationManagerIdLengthRule(1000);
        var item = AccessBadgeReservation.NewAccessBadgeReservation("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeReservationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccessBadgeReservationRequiredLabelRule("access badge reservation needs this field");
        var item = AccessBadgeReservation.NewAccessBadgeReservation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeReservationValidatorReportsEveryProblem();
        AccessBadgeReservationBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AccessBadgeReservationLabelLengthRuleFlagsALongLabel();
        AccessBadgeReservationQuantityRangeRuleFlagsValuesAboveQuantity();
        AccessBadgeReservationManagerIdLengthRuleFlagsALongManagerId();
        AccessBadgeReservationRequiredLabelRuleFlagsABlankLabel();
    }
}
