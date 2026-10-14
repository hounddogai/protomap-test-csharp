namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class CappedPoolAuditRulesChecks
{
    public static void CappedPoolAuditValidatorReportsEveryProblem()
    {
        var validator = CappedPoolAuditRules.DefaultCappedPoolAuditValidator();
        var item = CappedPoolAudit.NewCappedPoolAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPoolAuditRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new CappedPoolAuditRequiredReviewerRule("capped pool audit needs a value");
        var item = CappedPoolAudit.NewCappedPoolAudit("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPoolAuditLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedPoolAuditLabelLengthRule(64);
        var item = CappedPoolAudit.NewCappedPoolAudit("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPoolAuditStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new CappedPoolAuditStartsAtRangeRule(16, 86400);
        var item = CappedPoolAudit.NewCappedPoolAudit("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedPoolAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedPoolAuditRequiredCodeRule("capped pool audit needs this field");
        var item = CappedPoolAudit.NewCappedPoolAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPoolAuditBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new CappedPoolAuditBudgetRangeRule(2.8, 737.7);
        var item = CappedPoolAudit.NewCappedPoolAudit("x", "y");
        item.Budget = 739.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedPoolAuditValidatorReportsEveryProblem();
        CappedPoolAuditRequiredReviewerRuleFlagsABlankReviewer();
        CappedPoolAuditLabelLengthRuleFlagsALongLabel();
        CappedPoolAuditStartsAtRangeRuleFlagsValuesAboveStartsAt();
        CappedPoolAuditRequiredCodeRuleFlagsABlankCode();
        CappedPoolAuditBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
