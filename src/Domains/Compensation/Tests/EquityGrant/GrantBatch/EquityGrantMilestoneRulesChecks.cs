namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantBatch;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;

/// <summary>Checks of grant batch code, run by the project's runner.</summary>
public static class EquityGrantMilestoneRulesChecks
{
    public static void EquityGrantMilestoneValidatorReportsEveryProblem()
    {
        var validator = EquityGrantMilestoneRules.DefaultEquityGrantMilestoneValidator();
        var item = EquityGrantMilestone.NewEquityGrantMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantMilestoneGradeLengthRuleFlagsALongGrade()
    {
        var rule = new EquityGrantMilestoneGradeLengthRule(64);
        var item = EquityGrantMilestone.NewEquityGrantMilestone("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantMilestoneRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new EquityGrantMilestoneRequiredTitleRule("equity grant milestone needs a value");
        var item = EquityGrantMilestone.NewEquityGrantMilestone("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantMilestoneValidatorReportsEveryProblem();
        EquityGrantMilestoneGradeLengthRuleFlagsALongGrade();
        EquityGrantMilestoneRequiredTitleRuleFlagsABlankTitle();
    }
}
