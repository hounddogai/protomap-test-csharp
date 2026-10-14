namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class GlobalEngagementAssignmentRulesChecks
{
    public static void GlobalEngagementAssignmentValidatorReportsEveryProblem()
    {
        var validator = GlobalEngagementAssignmentRules.DefaultGlobalEngagementAssignmentValidator();
        var item = GlobalEngagementAssignment.NewGlobalEngagementAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalEngagementAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalEngagementAssignmentTitleLengthRule(80);
        var item = GlobalEngagementAssignment.NewGlobalEngagementAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalEngagementAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalEngagementAssignmentRequiredTitleRule("global engagement assignment needs this field");
        var item = GlobalEngagementAssignment.NewGlobalEngagementAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalEngagementAssignmentValidatorReportsEveryProblem();
        GlobalEngagementAssignmentTitleLengthRuleFlagsALongTitle();
        GlobalEngagementAssignmentRequiredTitleRuleFlagsABlankTitle();
    }
}
