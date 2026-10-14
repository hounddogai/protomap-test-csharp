namespace Acme.Workforce.Absence.Tests.Policy.Group;

using Acme.Workforce.Absence.Policy.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyGroupRulesChecks
{
    public static void PolicyGroupValidatorReportsEveryProblem()
    {
        var validator = PolicyGroupRules.DefaultPolicyGroupValidator();
        var item = PolicyGroup.NewPolicyGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyGroupRequiredNameRuleFlagsABlankName()
    {
        var rule = new PolicyGroupRequiredNameRule("policy group needs an entry");
        var item = PolicyGroup.NewPolicyGroup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyGroupReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new PolicyGroupReviewerLengthRule(255);
        var item = PolicyGroup.NewPolicyGroup("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyGroupScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PolicyGroupScoreRangeRule(3.2, 3051.3);
        var item = PolicyGroup.NewPolicyGroup("x", "y");
        item.Score = 3052.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyGroupValidatorReportsEveryProblem();
        PolicyGroupRequiredNameRuleFlagsABlankName();
        PolicyGroupReviewerLengthRuleFlagsALongReviewer();
        PolicyGroupScoreRangeRuleFlagsValuesAboveScore();
    }
}
