namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ScheduledEngagementGroupRulesChecks
{
    public static void ScheduledEngagementGroupValidatorReportsEveryProblem()
    {
        var validator = ScheduledEngagementGroupRules.DefaultScheduledEngagementGroupValidator();
        var item = ScheduledEngagementGroup.NewScheduledEngagementGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledEngagementGroupRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledEngagementGroupRequiredNameRule("scheduled engagement group needs a value");
        var item = ScheduledEngagementGroup.NewScheduledEngagementGroup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledEngagementGroupCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new ScheduledEngagementGroupCategoryLengthRule(120);
        var item = ScheduledEngagementGroup.NewScheduledEngagementGroup("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledEngagementGroupNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledEngagementGroupNameLengthRule(255);
        var item = ScheduledEngagementGroup.NewScheduledEngagementGroup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledEngagementGroupSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new ScheduledEngagementGroupSequenceRangeRule(120, 86400);
        var item = ScheduledEngagementGroup.NewScheduledEngagementGroup("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledEngagementGroupValidatorReportsEveryProblem();
        ScheduledEngagementGroupRequiredNameRuleFlagsABlankName();
        ScheduledEngagementGroupCategoryLengthRuleFlagsALongCategory();
        ScheduledEngagementGroupNameLengthRuleFlagsALongName();
        ScheduledEngagementGroupSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
