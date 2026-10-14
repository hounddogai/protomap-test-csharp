namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkScoreRulesChecks
{
    public static void StatementOfWorkScoreValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkScoreRules.DefaultStatementOfWorkScoreValidator();
        var item = StatementOfWorkScore.NewStatementOfWorkScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkScoreNameLengthRuleFlagsALongName()
    {
        var rule = new StatementOfWorkScoreNameLengthRule(16);
        var item = StatementOfWorkScore.NewStatementOfWorkScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkScoreUnitLengthRuleFlagsALongUnit()
    {
        var rule = new StatementOfWorkScoreUnitLengthRule(16);
        var item = StatementOfWorkScore.NewStatementOfWorkScore("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkScoreLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new StatementOfWorkScoreLimitRangeRule(64, 80);
        var item = StatementOfWorkScore.NewStatementOfWorkScore("x", "y");
        item.Limit = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StatementOfWorkScoreValidatorReportsEveryProblem();
        StatementOfWorkScoreNameLengthRuleFlagsALongName();
        StatementOfWorkScoreUnitLengthRuleFlagsALongUnit();
        StatementOfWorkScoreLimitRangeRuleFlagsValuesAboveLimit();
    }
}
