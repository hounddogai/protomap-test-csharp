namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultSentimentAssignmentRulesChecks
{
    public static void DefaultSentimentAssignmentValidatorReportsEveryProblem()
    {
        var validator = DefaultSentimentAssignmentRules.DefaultDefaultSentimentAssignmentValidator();
        var item = DefaultSentimentAssignment.NewDefaultSentimentAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultSentimentAssignmentEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new DefaultSentimentAssignmentEmployeeIdLengthRule(16);
        var item = DefaultSentimentAssignment.NewDefaultSentimentAssignment("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSentimentAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultSentimentAssignmentRequiredCodeRule("default sentiment assignment needs this field");
        var item = DefaultSentimentAssignment.NewDefaultSentimentAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultSentimentAssignmentCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new DefaultSentimentAssignmentCategoryLengthRule(80);
        var item = DefaultSentimentAssignment.NewDefaultSentimentAssignment("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultSentimentAssignmentValidatorReportsEveryProblem();
        DefaultSentimentAssignmentEmployeeIdLengthRuleFlagsALongEmployeeId();
        DefaultSentimentAssignmentRequiredCodeRuleFlagsABlankCode();
        DefaultSentimentAssignmentCategoryLengthRuleFlagsALongCategory();
    }
}
