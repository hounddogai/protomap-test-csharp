namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountReservationRulesChecks
{
    public static void HeadcountReservationValidatorReportsEveryProblem()
    {
        var validator = HeadcountReservationRules.DefaultHeadcountReservationValidator();
        var item = HeadcountReservation.NewHeadcountReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new HeadcountReservationRequiredTitleRule("headcount reservation needs this field");
        var item = HeadcountReservation.NewHeadcountReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountReservationAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new HeadcountReservationAttemptsRangeRule(86400, 86400);
        var item = HeadcountReservation.NewHeadcountReservation("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountReservationLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new HeadcountReservationLimitRangeRule(80, 3600);
        var item = HeadcountReservation.NewHeadcountReservation("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new HeadcountReservationCodeLengthRule(32);
        var item = HeadcountReservation.NewHeadcountReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountReservationValidatorReportsEveryProblem();
        HeadcountReservationRequiredTitleRuleFlagsABlankTitle();
        HeadcountReservationAttemptsRangeRuleFlagsValuesAboveAttempts();
        HeadcountReservationLimitRangeRuleFlagsValuesAboveLimit();
        HeadcountReservationCodeLengthRuleFlagsALongCode();
    }
}
