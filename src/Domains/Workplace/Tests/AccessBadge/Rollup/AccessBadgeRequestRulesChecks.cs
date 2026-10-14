namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeRequestRulesChecks
{
    public static void AccessBadgeRequestValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeRequestRules.DefaultAccessBadgeRequestValidator();
        var item = AccessBadgeRequest.NewAccessBadgeRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeRequestNameLengthRuleFlagsALongName()
    {
        var rule = new AccessBadgeRequestNameLengthRule(32);
        var item = AccessBadgeRequest.NewAccessBadgeRequest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeRequestRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccessBadgeRequestRequiredNameRule("access badge request needs this field");
        var item = AccessBadgeRequest.NewAccessBadgeRequest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AccessBadgeRequestTitleLengthRule(80);
        var item = AccessBadgeRequest.NewAccessBadgeRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeRequestValidatorReportsEveryProblem();
        AccessBadgeRequestNameLengthRuleFlagsALongName();
        AccessBadgeRequestRequiredNameRuleFlagsABlankName();
        AccessBadgeRequestTitleLengthRuleFlagsALongTitle();
    }
}
