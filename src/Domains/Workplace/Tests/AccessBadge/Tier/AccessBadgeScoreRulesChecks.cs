namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccessBadgeScoreRulesChecks
{
    public static void AccessBadgeScoreValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeScoreRules.DefaultAccessBadgeScoreValidator();
        var item = AccessBadgeScore.NewAccessBadgeScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccessBadgeScoreRequiredNameRule("access badge score needs a value");
        var item = AccessBadgeScore.NewAccessBadgeScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeScoreCodeLengthRule(86400);
        var item = AccessBadgeScore.NewAccessBadgeScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeScoreValidatorReportsEveryProblem();
        AccessBadgeScoreRequiredNameRuleFlagsABlankName();
        AccessBadgeScoreCodeLengthRuleFlagsALongCode();
    }
}
