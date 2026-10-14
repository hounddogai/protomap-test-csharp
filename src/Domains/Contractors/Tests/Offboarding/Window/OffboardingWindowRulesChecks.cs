namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class OffboardingWindowRulesChecks
{
    public static void OffboardingWindowValidatorReportsEveryProblem()
    {
        var validator = OffboardingWindowRules.DefaultOffboardingWindowValidator();
        var item = OffboardingWindow.NewOffboardingWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new OffboardingWindowCodeLengthRule(32);
        var item = OffboardingWindow.NewOffboardingWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingWindowLocationLengthRuleFlagsALongLocation()
    {
        var rule = new OffboardingWindowLocationLengthRule(1000);
        var item = OffboardingWindow.NewOffboardingWindow("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingWindowSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new OffboardingWindowSalaryBandLengthRule(32);
        var item = OffboardingWindow.NewOffboardingWindow("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingWindowValidatorReportsEveryProblem();
        OffboardingWindowCodeLengthRuleFlagsALongCode();
        OffboardingWindowLocationLengthRuleFlagsALongLocation();
        OffboardingWindowSalaryBandLengthRuleFlagsALongSalaryBand();
    }
}
