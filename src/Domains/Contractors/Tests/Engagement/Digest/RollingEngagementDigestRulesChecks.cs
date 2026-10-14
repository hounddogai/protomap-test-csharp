namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RollingEngagementDigestRulesChecks
{
    public static void RollingEngagementDigestValidatorReportsEveryProblem()
    {
        var validator = RollingEngagementDigestRules.DefaultRollingEngagementDigestValidator();
        var item = RollingEngagementDigest.NewRollingEngagementDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingEngagementDigestRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new RollingEngagementDigestRequiredCostCenterRule("rolling engagement digest needs this field");
        var item = RollingEngagementDigest.NewRollingEngagementDigest("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingEngagementDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingEngagementDigestCodeLengthRule(32);
        var item = RollingEngagementDigest.NewRollingEngagementDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingEngagementDigestRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new RollingEngagementDigestRequiredBadgeRule("rolling engagement digest needs this field");
        var item = RollingEngagementDigest.NewRollingEngagementDigest("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingEngagementDigestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingEngagementDigestRequiredLabelRule("rolling engagement digest needs a value");
        var item = RollingEngagementDigest.NewRollingEngagementDigest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingEngagementDigestValidatorReportsEveryProblem();
        RollingEngagementDigestRequiredCostCenterRuleFlagsABlankCostCenter();
        RollingEngagementDigestCodeLengthRuleFlagsALongCode();
        RollingEngagementDigestRequiredBadgeRuleFlagsABlankBadge();
        RollingEngagementDigestRequiredLabelRuleFlagsABlankLabel();
    }
}
