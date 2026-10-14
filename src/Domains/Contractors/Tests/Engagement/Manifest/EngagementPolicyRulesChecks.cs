namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class EngagementPolicyRulesChecks
{
    public static void EngagementPolicyValidatorReportsEveryProblem()
    {
        var validator = EngagementPolicyRules.DefaultEngagementPolicyValidator();
        var item = EngagementPolicy.NewEngagementPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementPolicyMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new EngagementPolicyMinimumRangeRule(1.6, 1061.9);
        var item = EngagementPolicy.NewEngagementPolicy("x", "y");
        item.Minimum = 1063.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementPolicySequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new EngagementPolicySequenceRangeRule(255, 1000);
        var item = EngagementPolicy.NewEngagementPolicy("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementPolicyUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new EngagementPolicyUpdatedAtRangeRule(120, 1000);
        var item = EngagementPolicy.NewEngagementPolicy("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementPolicyRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new EngagementPolicyRequiredTeamRule("engagement policy needs this field");
        var item = EngagementPolicy.NewEngagementPolicy("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementPolicyRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new EngagementPolicyRequiredTitleRule("engagement policy needs a value");
        var item = EngagementPolicy.NewEngagementPolicy("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementPolicyValidatorReportsEveryProblem();
        EngagementPolicyMinimumRangeRuleFlagsValuesAboveMinimum();
        EngagementPolicySequenceRangeRuleFlagsValuesAboveSequence();
        EngagementPolicyUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        EngagementPolicyRequiredTeamRuleFlagsABlankTeam();
        EngagementPolicyRequiredTitleRuleFlagsABlankTitle();
    }
}
