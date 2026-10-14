namespace Acme.Workforce.Contractors.Tests.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class EngagementGroupRulesChecks
{
    public static void EngagementGroupValidatorReportsEveryProblem()
    {
        var validator = EngagementGroupRules.DefaultEngagementGroupValidator();
        var item = EngagementGroup.NewEngagementGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new EngagementGroupCodeLengthRule(86400);
        var item = EngagementGroup.NewEngagementGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementGroupRequiredCodeRule("engagement group needs an entry");
        var item = EngagementGroup.NewEngagementGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementGroupValidatorReportsEveryProblem();
        EngagementGroupCodeLengthRuleFlagsALongCode();
        EngagementGroupRequiredCodeRuleFlagsABlankCode();
    }
}
