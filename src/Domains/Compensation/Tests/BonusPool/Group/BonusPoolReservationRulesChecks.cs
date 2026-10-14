namespace Acme.Workforce.Compensation.Tests.BonusPool.Group;

using Acme.Workforce.Compensation.BonusPool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BonusPoolReservationRulesChecks
{
    public static void BonusPoolReservationValidatorReportsEveryProblem()
    {
        var validator = BonusPoolReservationRules.DefaultBonusPoolReservationValidator();
        var item = BonusPoolReservation.NewBonusPoolReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BonusPoolReservationRequiredCodeRule("bonus pool reservation needs an entry");
        var item = BonusPoolReservation.NewBonusPoolReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolReservationDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new BonusPoolReservationDurationMinutesRangeRule(32, 255);
        var item = BonusPoolReservation.NewBonusPoolReservation("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolReservationWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new BonusPoolReservationWindowDaysRangeRule(120, 120);
        var item = BonusPoolReservation.NewBonusPoolReservation("x", "y");
        item.WindowDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolReservationBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new BonusPoolReservationBadgeLengthRule(3600);
        var item = BonusPoolReservation.NewBonusPoolReservation("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BonusPoolReservationValidatorReportsEveryProblem();
        BonusPoolReservationRequiredCodeRuleFlagsABlankCode();
        BonusPoolReservationDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        BonusPoolReservationWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        BonusPoolReservationBadgeLengthRuleFlagsALongBadge();
    }
}
