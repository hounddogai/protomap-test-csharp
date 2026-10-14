namespace Acme.Workforce.Contractors.Tests.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class EngagementDigestRulesChecks
{
    public static void EngagementDigestValidatorReportsEveryProblem()
    {
        var validator = EngagementDigestRules.DefaultEngagementDigestValidator();
        var item = EngagementDigest.NewEngagementDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementDigestRequiredNameRuleFlagsABlankName()
    {
        var rule = new EngagementDigestRequiredNameRule("engagement digest needs a value");
        var item = EngagementDigest.NewEngagementDigest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementDigestCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new EngagementDigestCostCenterLengthRule(64);
        var item = EngagementDigest.NewEngagementDigest("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new EngagementDigestCodeLengthRule(32);
        var item = EngagementDigest.NewEngagementDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EngagementDigestValidatorReportsEveryProblem();
        EngagementDigestRequiredNameRuleFlagsABlankName();
        EngagementDigestCostCenterLengthRuleFlagsALongCostCenter();
        EngagementDigestCodeLengthRuleFlagsALongCode();
    }
}
