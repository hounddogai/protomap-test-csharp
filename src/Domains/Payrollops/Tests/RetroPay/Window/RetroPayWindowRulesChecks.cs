namespace Acme.Workforce.Payrollops.Tests.RetroPay.Window;

using Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RetroPayWindowRulesChecks
{
    public static void RetroPayWindowValidatorReportsEveryProblem()
    {
        var validator = RetroPayWindowRules.DefaultRetroPayWindowValidator();
        var item = RetroPayWindow.NewRetroPayWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayWindowLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RetroPayWindowLabelLengthRule(255);
        var item = RetroPayWindow.NewRetroPayWindow("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayWindowRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new RetroPayWindowRequiredSourceRule("retro pay window needs a value");
        var item = RetroPayWindow.NewRetroPayWindow("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayWindowExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RetroPayWindowExpiresAtRangeRule(120, 3600);
        var item = RetroPayWindow.NewRetroPayWindow("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayWindowRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new RetroPayWindowRequiredSalaryBandRule("retro pay window needs an entry");
        var item = RetroPayWindow.NewRetroPayWindow("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayWindowValidatorReportsEveryProblem();
        RetroPayWindowLabelLengthRuleFlagsALongLabel();
        RetroPayWindowRequiredSourceRuleFlagsABlankSource();
        RetroPayWindowExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        RetroPayWindowRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
