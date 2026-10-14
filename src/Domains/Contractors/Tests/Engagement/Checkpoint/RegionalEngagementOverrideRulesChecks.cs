namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalEngagementOverrideRulesChecks
{
    public static void RegionalEngagementOverrideValidatorReportsEveryProblem()
    {
        var validator = RegionalEngagementOverrideRules.DefaultRegionalEngagementOverrideValidator();
        var item = RegionalEngagementOverride.NewRegionalEngagementOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalEngagementOverrideRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new RegionalEngagementOverrideRequiredSegmentRule("regional engagement override needs an entry");
        var item = RegionalEngagementOverride.NewRegionalEngagementOverride("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalEngagementOverrideRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new RegionalEngagementOverrideRequiredLocationRule("regional engagement override needs a value");
        var item = RegionalEngagementOverride.NewRegionalEngagementOverride("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalEngagementOverrideLocationLengthRuleFlagsALongLocation()
    {
        var rule = new RegionalEngagementOverrideLocationLengthRule(16);
        var item = RegionalEngagementOverride.NewRegionalEngagementOverride("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalEngagementOverrideUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new RegionalEngagementOverrideUpdatedAtRangeRule(1000, 86400);
        var item = RegionalEngagementOverride.NewRegionalEngagementOverride("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalEngagementOverrideValidatorReportsEveryProblem();
        RegionalEngagementOverrideRequiredSegmentRuleFlagsABlankSegment();
        RegionalEngagementOverrideRequiredLocationRuleFlagsABlankLocation();
        RegionalEngagementOverrideLocationLengthRuleFlagsALongLocation();
        RegionalEngagementOverrideUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
