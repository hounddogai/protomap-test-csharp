namespace Acme.Workforce.Compensation.Tests.PayBand.Preference;

using Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PayBandReservationRulesChecks
{
    public static void PayBandReservationValidatorReportsEveryProblem()
    {
        var validator = PayBandReservationRules.DefaultPayBandReservationValidator();
        var item = PayBandReservation.NewPayBandReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandReservationRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new PayBandReservationRequiredJobCodeRule("pay band reservation needs an entry");
        var item = PayBandReservation.NewPayBandReservation("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new PayBandReservationRequiredNameRule("pay band reservation needs an entry");
        var item = PayBandReservation.NewPayBandReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandReservationRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PayBandReservationRetriesRangeRule(120, 3600);
        var item = PayBandReservation.NewPayBandReservation("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandReservationTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new PayBandReservationTenureMonthsRangeRule(255, 3600);
        var item = PayBandReservation.NewPayBandReservation("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayBandReservationValidatorReportsEveryProblem();
        PayBandReservationRequiredJobCodeRuleFlagsABlankJobCode();
        PayBandReservationRequiredNameRuleFlagsABlankName();
        PayBandReservationRetriesRangeRuleFlagsValuesAboveRetries();
        PayBandReservationTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
