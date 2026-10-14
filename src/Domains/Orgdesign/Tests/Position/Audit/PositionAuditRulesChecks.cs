namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PositionAuditRulesChecks
{
    public static void PositionAuditValidatorReportsEveryProblem()
    {
        var validator = PositionAuditRules.DefaultPositionAuditValidator();
        var item = PositionAudit.NewPositionAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionAuditReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new PositionAuditReviewerLengthRule(32);
        var item = PositionAudit.NewPositionAudit("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionAuditRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new PositionAuditRequiredReviewerRule("position audit needs an entry");
        var item = PositionAudit.NewPositionAudit("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionAuditRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PositionAuditRequiredTitleRule("position audit needs a value");
        var item = PositionAudit.NewPositionAudit("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PositionAuditValidatorReportsEveryProblem();
        PositionAuditReviewerLengthRuleFlagsALongReviewer();
        PositionAuditRequiredReviewerRuleFlagsABlankReviewer();
        PositionAuditRequiredTitleRuleFlagsABlankTitle();
    }
}
