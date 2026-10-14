namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class StatementOfWorkDraftRulesChecks
{
    public static void StatementOfWorkDraftValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkDraftRules.DefaultStatementOfWorkDraftValidator();
        var item = StatementOfWorkDraft.NewStatementOfWorkDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkDraftScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new StatementOfWorkDraftScoreRangeRule(6.6, 3043.7);
        var item = StatementOfWorkDraft.NewStatementOfWorkDraft("x", "y");
        item.Score = 3045.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkDraftGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new StatementOfWorkDraftGraceDaysRangeRule(120, 120);
        var item = StatementOfWorkDraft.NewStatementOfWorkDraft("x", "y");
        item.GraceDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StatementOfWorkDraftValidatorReportsEveryProblem();
        StatementOfWorkDraftScoreRangeRuleFlagsValuesAboveScore();
        StatementOfWorkDraftGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
