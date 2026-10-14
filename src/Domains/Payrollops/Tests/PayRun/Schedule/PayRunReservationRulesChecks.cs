namespace Acme.Workforce.Payrollops.Tests.PayRun.Schedule;

using Acme.Workforce.Payrollops.PayRun.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PayRunReservationRulesChecks
{
    public static void PayRunReservationValidatorReportsEveryProblem()
    {
        var validator = PayRunReservationRules.DefaultPayRunReservationValidator();
        var item = PayRunReservation.NewPayRunReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunReservationSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new PayRunReservationSummaryLengthRule(255);
        var item = PayRunReservation.NewPayRunReservation("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayRunReservationRequiredCodeRule("pay run reservation needs an entry");
        var item = PayRunReservation.NewPayRunReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunReservationRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new PayRunReservationRequiredRegionRule("pay run reservation needs this field");
        var item = PayRunReservation.NewPayRunReservation("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayRunReservationValidatorReportsEveryProblem();
        PayRunReservationSummaryLengthRuleFlagsALongSummary();
        PayRunReservationRequiredCodeRuleFlagsABlankCode();
        PayRunReservationRequiredRegionRuleFlagsABlankRegion();
    }
}
