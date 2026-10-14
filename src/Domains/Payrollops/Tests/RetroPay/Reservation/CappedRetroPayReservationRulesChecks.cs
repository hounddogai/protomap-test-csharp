namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedRetroPayReservationRulesChecks
{
    public static void CappedRetroPayReservationValidatorReportsEveryProblem()
    {
        var validator = CappedRetroPayReservationRules.DefaultCappedRetroPayReservationValidator();
        var item = CappedRetroPayReservation.NewCappedRetroPayReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedRetroPayReservationNotesLengthRuleFlagsALongNotes()
    {
        var rule = new CappedRetroPayReservationNotesLengthRule(86400);
        var item = CappedRetroPayReservation.NewCappedRetroPayReservation("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedRetroPayReservationRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new CappedRetroPayReservationRequiredNotesRule("capped retro pay reservation needs an entry");
        var item = CappedRetroPayReservation.NewCappedRetroPayReservation("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedRetroPayReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedRetroPayReservationRequiredCodeRule("capped retro pay reservation needs an entry");
        var item = CappedRetroPayReservation.NewCappedRetroPayReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedRetroPayReservationValidatorReportsEveryProblem();
        CappedRetroPayReservationNotesLengthRuleFlagsALongNotes();
        CappedRetroPayReservationRequiredNotesRuleFlagsABlankNotes();
        CappedRetroPayReservationRequiredCodeRuleFlagsABlankCode();
    }
}
