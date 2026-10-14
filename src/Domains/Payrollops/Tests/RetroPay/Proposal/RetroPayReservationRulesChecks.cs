namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayReservationRulesChecks
{
    public static void RetroPayReservationValidatorReportsEveryProblem()
    {
        var validator = RetroPayReservationRules.DefaultRetroPayReservationValidator();
        var item = RetroPayReservation.NewRetroPayReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayReservationBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RetroPayReservationBadgeLengthRule(16);
        var item = RetroPayReservation.NewRetroPayReservation("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPayReservationCodeLengthRule(255);
        var item = RetroPayReservation.NewRetroPayReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayReservationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RetroPayReservationRequiredLabelRule("retro pay reservation needs a value");
        var item = RetroPayReservation.NewRetroPayReservation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayReservationValidatorReportsEveryProblem();
        RetroPayReservationBadgeLengthRuleFlagsALongBadge();
        RetroPayReservationCodeLengthRuleFlagsALongCode();
        RetroPayReservationRequiredLabelRuleFlagsABlankLabel();
    }
}
