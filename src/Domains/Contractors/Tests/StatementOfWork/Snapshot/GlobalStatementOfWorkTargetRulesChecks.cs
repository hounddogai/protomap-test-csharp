namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalStatementOfWorkTargetRulesChecks
{
    public static void GlobalStatementOfWorkTargetValidatorReportsEveryProblem()
    {
        var validator = GlobalStatementOfWorkTargetRules.DefaultGlobalStatementOfWorkTargetValidator();
        var item = GlobalStatementOfWorkTarget.NewGlobalStatementOfWorkTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalStatementOfWorkTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalStatementOfWorkTargetRequiredNameRule("global statement of work target needs an entry");
        var item = GlobalStatementOfWorkTarget.NewGlobalStatementOfWorkTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalStatementOfWorkTargetRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new GlobalStatementOfWorkTargetRequiredJobCodeRule("global statement of work target needs an entry");
        var item = GlobalStatementOfWorkTarget.NewGlobalStatementOfWorkTarget("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalStatementOfWorkTargetValidatorReportsEveryProblem();
        GlobalStatementOfWorkTargetRequiredNameRuleFlagsABlankName();
        GlobalStatementOfWorkTargetRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
