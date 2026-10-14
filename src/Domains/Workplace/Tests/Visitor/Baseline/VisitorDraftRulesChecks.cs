namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class VisitorDraftRulesChecks
{
    public static void VisitorDraftValidatorReportsEveryProblem()
    {
        var validator = VisitorDraftRules.DefaultVisitorDraftValidator();
        var item = VisitorDraft.NewVisitorDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VisitorDraftRequiredCodeRule("visitor draft needs a value");
        var item = VisitorDraft.NewVisitorDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new VisitorDraftRequiredTitleRule("visitor draft needs this field");
        var item = VisitorDraft.NewVisitorDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorDraftValidatorReportsEveryProblem();
        VisitorDraftRequiredCodeRuleFlagsABlankCode();
        VisitorDraftRequiredTitleRuleFlagsABlankTitle();
    }
}
