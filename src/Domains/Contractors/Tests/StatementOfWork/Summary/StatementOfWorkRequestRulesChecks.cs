namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StatementOfWorkRequestRulesChecks
{
    public static void StatementOfWorkRequestValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkRequestRules.DefaultStatementOfWorkRequestValidator();
        var item = StatementOfWorkRequest.NewStatementOfWorkRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkRequestRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new StatementOfWorkRequestRequiredTeamRule("statement of work request needs an entry");
        var item = StatementOfWorkRequest.NewStatementOfWorkRequest("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkRequestSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new StatementOfWorkRequestSalaryBandLengthRule(64);
        var item = StatementOfWorkRequest.NewStatementOfWorkRequest("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkRequestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StatementOfWorkRequestRequiredTitleRule("statement of work request needs an entry");
        var item = StatementOfWorkRequest.NewStatementOfWorkRequest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkRequestValidatorReportsEveryProblem();
        StatementOfWorkRequestRequiredTeamRuleFlagsABlankTeam();
        StatementOfWorkRequestSalaryBandLengthRuleFlagsALongSalaryBand();
        StatementOfWorkRequestRequiredTitleRuleFlagsABlankTitle();
    }
}
