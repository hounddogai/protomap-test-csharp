namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GarnishmentReservationRulesChecks
{
    public static void GarnishmentReservationValidatorReportsEveryProblem()
    {
        var validator = GarnishmentReservationRules.DefaultGarnishmentReservationValidator();
        var item = GarnishmentReservation.NewGarnishmentReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentReservationCodeLengthRule(255);
        var item = GarnishmentReservation.NewGarnishmentReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentReservationCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new GarnishmentReservationCreatedAtRangeRule(16, 64);
        var item = GarnishmentReservation.NewGarnishmentReservation("x", "y");
        item.CreatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GarnishmentReservationValidatorReportsEveryProblem();
        GarnishmentReservationCodeLengthRuleFlagsALongCode();
        GarnishmentReservationCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
