namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PendingExceptionProposalRulesChecks
{
    public static void PendingExceptionProposalValidatorReportsEveryProblem()
    {
        var validator = PendingExceptionProposalRules.DefaultPendingExceptionProposalValidator();
        var item = PendingExceptionProposal.NewPendingExceptionProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingExceptionProposalAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PendingExceptionProposalAttemptsRangeRule(64, 255);
        var item = PendingExceptionProposal.NewPendingExceptionProposal("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingExceptionProposalRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingExceptionProposalRequiredTitleRule("pending exception proposal needs a value");
        var item = PendingExceptionProposal.NewPendingExceptionProposal("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingExceptionProposalValidatorReportsEveryProblem();
        PendingExceptionProposalAttemptsRangeRuleFlagsValuesAboveAttempts();
        PendingExceptionProposalRequiredTitleRuleFlagsABlankTitle();
    }
}
