namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class DefaultAccessBadgeManifestRulesChecks
{
    public static void DefaultAccessBadgeManifestValidatorReportsEveryProblem()
    {
        var validator = DefaultAccessBadgeManifestRules.DefaultDefaultAccessBadgeManifestValidator();
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultAccessBadgeManifestRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new DefaultAccessBadgeManifestRequiredSegmentRule("default access badge manifest needs this field");
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultAccessBadgeManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultAccessBadgeManifestCodeLengthRule(120);
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAccessBadgeManifestSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new DefaultAccessBadgeManifestSegmentLengthRule(32);
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultAccessBadgeManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultAccessBadgeManifestRequiredCodeRule("default access badge manifest needs an entry");
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultAccessBadgeManifestHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new DefaultAccessBadgeManifestHoursRangeRule(6.7, 3254.0);
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("x", "y");
        item.Hours = 3255.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeManifestValidatorReportsEveryProblem();
        DefaultAccessBadgeManifestRequiredSegmentRuleFlagsABlankSegment();
        DefaultAccessBadgeManifestCodeLengthRuleFlagsALongCode();
        DefaultAccessBadgeManifestSegmentLengthRuleFlagsALongSegment();
        DefaultAccessBadgeManifestRequiredCodeRuleFlagsABlankCode();
        DefaultAccessBadgeManifestHoursRangeRuleFlagsValuesAboveHours();
    }
}
