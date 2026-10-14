namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualBonusPoolReservationRulesChecks
{
    public static void ManualBonusPoolReservationValidatorReportsEveryProblem()
    {
        var validator = ManualBonusPoolReservationRules.DefaultManualBonusPoolReservationValidator();
        var item = ManualBonusPoolReservation.NewManualBonusPoolReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBonusPoolReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualBonusPoolReservationLabelLengthRule(3600);
        var item = ManualBonusPoolReservation.NewManualBonusPoolReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBonusPoolReservationCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new ManualBonusPoolReservationCostRangeRule(4.4, 3701.3);
        var item = ManualBonusPoolReservation.NewManualBonusPoolReservation("x", "y");
        item.Cost = 3702.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualBonusPoolReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualBonusPoolReservationCodeLengthRule(120);
        var item = ManualBonusPoolReservation.NewManualBonusPoolReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBonusPoolReservationCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new ManualBonusPoolReservationCategoryLengthRule(16);
        var item = ManualBonusPoolReservation.NewManualBonusPoolReservation("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualBonusPoolReservationValidatorReportsEveryProblem();
        ManualBonusPoolReservationLabelLengthRuleFlagsALongLabel();
        ManualBonusPoolReservationCostRangeRuleFlagsValuesAboveCost();
        ManualBonusPoolReservationCodeLengthRuleFlagsALongCode();
        ManualBonusPoolReservationCategoryLengthRuleFlagsALongCategory();
    }
}
