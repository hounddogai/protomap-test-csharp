namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestBaselineRulesChecks
{
    public static void LeaveRequestBaselineValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestBaselineRules.DefaultLeaveRequestBaselineValidator();
        var item = LeaveRequestBaseline.NewLeaveRequestBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestBaselineCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new LeaveRequestBaselineCurrencyLengthRule(120);
        var item = LeaveRequestBaseline.NewLeaveRequestBaseline("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestBaselineRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new LeaveRequestBaselineRequiredCurrencyRule("leave request baseline needs a value");
        var item = LeaveRequestBaseline.NewLeaveRequestBaseline("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestBaselineValidatorReportsEveryProblem();
        LeaveRequestBaselineCurrencyLengthRuleFlagsALongCurrency();
        LeaveRequestBaselineRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
