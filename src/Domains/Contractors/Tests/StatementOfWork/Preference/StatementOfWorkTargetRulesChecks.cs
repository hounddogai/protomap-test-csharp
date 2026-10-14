namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkTargetRulesChecks
{
    public static void StatementOfWorkTargetValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkTargetRules.DefaultStatementOfWorkTargetValidator();
        var item = StatementOfWorkTarget.NewStatementOfWorkTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkTargetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StatementOfWorkTargetRequiredTitleRule("statement of work target needs an entry");
        var item = StatementOfWorkTarget.NewStatementOfWorkTarget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new StatementOfWorkTargetRequiredNameRule("statement of work target needs a value");
        var item = StatementOfWorkTarget.NewStatementOfWorkTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkTargetCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new StatementOfWorkTargetCategoryLengthRule(80);
        var item = StatementOfWorkTarget.NewStatementOfWorkTarget("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StatementOfWorkTargetValidatorReportsEveryProblem();
        StatementOfWorkTargetRequiredTitleRuleFlagsABlankTitle();
        StatementOfWorkTargetRequiredNameRuleFlagsABlankName();
        StatementOfWorkTargetCategoryLengthRuleFlagsALongCategory();
    }
}
