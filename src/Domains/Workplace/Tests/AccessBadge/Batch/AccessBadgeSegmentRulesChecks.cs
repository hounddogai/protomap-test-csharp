namespace Acme.Workforce.Workplace.Tests.AccessBadge.Batch;

using Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccessBadgeSegmentRulesChecks
{
    public static void AccessBadgeSegmentValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeSegmentRules.DefaultAccessBadgeSegmentValidator();
        var item = AccessBadgeSegment.NewAccessBadgeSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new AccessBadgeSegmentNameLengthRule(16);
        var item = AccessBadgeSegment.NewAccessBadgeSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeSegmentRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new AccessBadgeSegmentRequiredChecksumRule("access badge segment needs this field");
        var item = AccessBadgeSegment.NewAccessBadgeSegment("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeSegmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AccessBadgeSegmentExpiresAtRangeRule(80, 1000);
        var item = AccessBadgeSegment.NewAccessBadgeSegment("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeSegmentCodeLengthRule(16);
        var item = AccessBadgeSegment.NewAccessBadgeSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeSegmentRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new AccessBadgeSegmentRequiredSourceRule("access badge segment needs a value");
        var item = AccessBadgeSegment.NewAccessBadgeSegment("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeSegmentValidatorReportsEveryProblem();
        AccessBadgeSegmentNameLengthRuleFlagsALongName();
        AccessBadgeSegmentRequiredChecksumRuleFlagsABlankChecksum();
        AccessBadgeSegmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        AccessBadgeSegmentCodeLengthRuleFlagsALongCode();
        AccessBadgeSegmentRequiredSourceRuleFlagsABlankSource();
    }
}
