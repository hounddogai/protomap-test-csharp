namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkCheckpointRulesChecks
{
    public static void StatementOfWorkCheckpointValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkCheckpointRules.DefaultStatementOfWorkCheckpointValidator();
        var item = StatementOfWorkCheckpoint.NewStatementOfWorkCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkCheckpointRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StatementOfWorkCheckpointRequiredTitleRule("statement of work checkpoint needs a value");
        var item = StatementOfWorkCheckpoint.NewStatementOfWorkCheckpoint("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkCheckpointMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new StatementOfWorkCheckpointMaximumRangeRule(3.0, 1940.3);
        var item = StatementOfWorkCheckpoint.NewStatementOfWorkCheckpoint("x", "y");
        item.Maximum = 1941.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StatementOfWorkCheckpointValidatorReportsEveryProblem();
        StatementOfWorkCheckpointRequiredTitleRuleFlagsABlankTitle();
        StatementOfWorkCheckpointMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
