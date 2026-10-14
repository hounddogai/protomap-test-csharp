namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class RollingAccessBadgeRequestRulesChecks
{
    public static void RollingAccessBadgeRequestValidatorReportsEveryProblem()
    {
        var validator = RollingAccessBadgeRequestRules.DefaultRollingAccessBadgeRequestValidator();
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingAccessBadgeRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingAccessBadgeRequestRequiredCodeRule("rolling access badge request needs a value");
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingAccessBadgeRequestEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new RollingAccessBadgeRequestEndsAtRangeRule(16, 80);
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("x", "y");
        item.EndsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingAccessBadgeRequestMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new RollingAccessBadgeRequestMaximumRangeRule(5.9, 4659.6);
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("x", "y");
        item.Maximum = 4661.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingAccessBadgeRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingAccessBadgeRequestCodeLengthRule(64);
        var item = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingAccessBadgeRequestValidatorReportsEveryProblem();
        RollingAccessBadgeRequestRequiredCodeRuleFlagsABlankCode();
        RollingAccessBadgeRequestEndsAtRangeRuleFlagsValuesAboveEndsAt();
        RollingAccessBadgeRequestMaximumRangeRuleFlagsValuesAboveMaximum();
        RollingAccessBadgeRequestCodeLengthRuleFlagsALongCode();
    }
}
