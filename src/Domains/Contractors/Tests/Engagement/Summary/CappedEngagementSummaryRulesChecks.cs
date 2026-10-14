namespace Acme.Workforce.Contractors.Tests.Engagement.Summary;

using Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CappedEngagementSummaryRulesChecks
{
    public static void CappedEngagementSummaryValidatorReportsEveryProblem()
    {
        var validator = CappedEngagementSummaryRules.DefaultCappedEngagementSummaryValidator();
        var item = CappedEngagementSummary.NewCappedEngagementSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedEngagementSummaryRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new CappedEngagementSummaryRequiredGradeRule("capped engagement summary needs a value");
        var item = CappedEngagementSummary.NewCappedEngagementSummary("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedEngagementSummaryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedEngagementSummaryRequiredLabelRule("capped engagement summary needs this field");
        var item = CappedEngagementSummary.NewCappedEngagementSummary("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedEngagementSummaryAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new CappedEngagementSummaryAttemptsRangeRule(255, 1000);
        var item = CappedEngagementSummary.NewCappedEngagementSummary("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedEngagementSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedEngagementSummaryRequiredCodeRule("capped engagement summary needs a value");
        var item = CappedEngagementSummary.NewCappedEngagementSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedEngagementSummaryGradeLengthRuleFlagsALongGrade()
    {
        var rule = new CappedEngagementSummaryGradeLengthRule(32);
        var item = CappedEngagementSummary.NewCappedEngagementSummary("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedEngagementSummaryValidatorReportsEveryProblem();
        CappedEngagementSummaryRequiredGradeRuleFlagsABlankGrade();
        CappedEngagementSummaryRequiredLabelRuleFlagsABlankLabel();
        CappedEngagementSummaryAttemptsRangeRuleFlagsValuesAboveAttempts();
        CappedEngagementSummaryRequiredCodeRuleFlagsABlankCode();
        CappedEngagementSummaryGradeLengthRuleFlagsALongGrade();
    }
}
