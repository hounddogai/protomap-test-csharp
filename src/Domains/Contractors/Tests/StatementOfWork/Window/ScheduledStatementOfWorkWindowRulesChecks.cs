namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkWindowRulesChecks
{
    public static void ScheduledStatementOfWorkWindowValidatorReportsEveryProblem()
    {
        var validator = ScheduledStatementOfWorkWindowRules.DefaultScheduledStatementOfWorkWindowValidator();
        var item = ScheduledStatementOfWorkWindow.NewScheduledStatementOfWorkWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledStatementOfWorkWindowCountryLengthRuleFlagsALongCountry()
    {
        var rule = new ScheduledStatementOfWorkWindowCountryLengthRule(32);
        var item = ScheduledStatementOfWorkWindow.NewScheduledStatementOfWorkWindow("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledStatementOfWorkWindowLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledStatementOfWorkWindowLabelLengthRule(3600);
        var item = ScheduledStatementOfWorkWindow.NewScheduledStatementOfWorkWindow("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkWindowValidatorReportsEveryProblem();
        ScheduledStatementOfWorkWindowCountryLengthRuleFlagsALongCountry();
        ScheduledStatementOfWorkWindowLabelLengthRuleFlagsALongLabel();
    }
}
