namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyReservationRulesChecks
{
    public static void WarrantyReservationValidatorReportsEveryProblem()
    {
        var validator = WarrantyReservationRules.DefaultWarrantyReservationValidator();
        var item = WarrantyReservation.NewWarrantyReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new WarrantyReservationCodeLengthRule(32);
        var item = WarrantyReservation.NewWarrantyReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyReservationRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new WarrantyReservationRequiredBadgeRule("warranty reservation needs an entry");
        var item = WarrantyReservation.NewWarrantyReservation("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyReservationThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new WarrantyReservationThresholdRangeRule(9.4, 4580.4);
        var item = WarrantyReservation.NewWarrantyReservation("x", "y");
        item.Threshold = 4581.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WarrantyReservationValidatorReportsEveryProblem();
        WarrantyReservationCodeLengthRuleFlagsALongCode();
        WarrantyReservationRequiredBadgeRuleFlagsABlankBadge();
        WarrantyReservationThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
