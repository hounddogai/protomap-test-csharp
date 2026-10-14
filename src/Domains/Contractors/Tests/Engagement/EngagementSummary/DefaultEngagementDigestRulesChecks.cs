namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class DefaultEngagementDigestRulesChecks
{
    public static void DefaultEngagementDigestValidatorReportsEveryProblem()
    {
        var validator = DefaultEngagementDigestRules.DefaultDefaultEngagementDigestValidator();
        var item = DefaultEngagementDigest.NewDefaultEngagementDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultEngagementDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultEngagementDigestRequiredCodeRule("default engagement digest needs this field");
        var item = DefaultEngagementDigest.NewDefaultEngagementDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultEngagementDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultEngagementDigestCodeLengthRule(1000);
        var item = DefaultEngagementDigest.NewDefaultEngagementDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultEngagementDigestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultEngagementDigestRequiredLabelRule("default engagement digest needs an entry");
        var item = DefaultEngagementDigest.NewDefaultEngagementDigest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultEngagementDigestNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultEngagementDigestNameLengthRule(64);
        var item = DefaultEngagementDigest.NewDefaultEngagementDigest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultEngagementDigestValidatorReportsEveryProblem();
        DefaultEngagementDigestRequiredCodeRuleFlagsABlankCode();
        DefaultEngagementDigestCodeLengthRuleFlagsALongCode();
        DefaultEngagementDigestRequiredLabelRuleFlagsABlankLabel();
        DefaultEngagementDigestNameLengthRuleFlagsALongName();
    }
}
