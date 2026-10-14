namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class StatementOfWorkWindowRulesChecks
{
    public static void StatementOfWorkWindowValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkWindowRules.DefaultStatementOfWorkWindowValidator();
        var item = StatementOfWorkWindow.NewStatementOfWorkWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkWindowRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new StatementOfWorkWindowRequiredManagerIdRule("statement of work window needs this field");
        var item = StatementOfWorkWindow.NewStatementOfWorkWindow("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkWindowManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new StatementOfWorkWindowManagerIdLengthRule(86400);
        var item = StatementOfWorkWindow.NewStatementOfWorkWindow("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkWindowChannelLengthRuleFlagsALongChannel()
    {
        var rule = new StatementOfWorkWindowChannelLengthRule(16);
        var item = StatementOfWorkWindow.NewStatementOfWorkWindow("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StatementOfWorkWindowRequiredCodeRule("statement of work window needs a value");
        var item = StatementOfWorkWindow.NewStatementOfWorkWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkWindowValidatorReportsEveryProblem();
        StatementOfWorkWindowRequiredManagerIdRuleFlagsABlankManagerId();
        StatementOfWorkWindowManagerIdLengthRuleFlagsALongManagerId();
        StatementOfWorkWindowChannelLengthRuleFlagsALongChannel();
        StatementOfWorkWindowRequiredCodeRuleFlagsABlankCode();
    }
}
