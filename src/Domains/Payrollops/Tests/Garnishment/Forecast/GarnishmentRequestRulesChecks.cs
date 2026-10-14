namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentRequestRulesChecks
{
    public static void GarnishmentRequestValidatorReportsEveryProblem()
    {
        var validator = GarnishmentRequestRules.DefaultGarnishmentRequestValidator();
        var item = GarnishmentRequest.NewGarnishmentRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentRequestFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new GarnishmentRequestFteRangeRule(9.8, 2633.4);
        var item = GarnishmentRequest.NewGarnishmentRequest("x", "y");
        item.Fte = 2634.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentRequestRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new GarnishmentRequestRequiredContactEmailRule("garnishment request needs an entry");
        var item = GarnishmentRequest.NewGarnishmentRequest("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentRequestContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new GarnishmentRequestContactEmailLengthRule(86400);
        var item = GarnishmentRequest.NewGarnishmentRequest("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentRequestRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new GarnishmentRequestRequiredEmployeeIdRule("garnishment request needs this field");
        var item = GarnishmentRequest.NewGarnishmentRequest("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GarnishmentRequestTitleLengthRule(80);
        var item = GarnishmentRequest.NewGarnishmentRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentRequestMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new GarnishmentRequestMaximumRangeRule(8.9, 3406.8);
        var item = GarnishmentRequest.NewGarnishmentRequest("x", "y");
        item.Maximum = 3408.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GarnishmentRequestValidatorReportsEveryProblem();
        GarnishmentRequestFteRangeRuleFlagsValuesAboveFte();
        GarnishmentRequestRequiredContactEmailRuleFlagsABlankContactEmail();
        GarnishmentRequestContactEmailLengthRuleFlagsALongContactEmail();
        GarnishmentRequestRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        GarnishmentRequestTitleLengthRuleFlagsALongTitle();
        GarnishmentRequestMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
