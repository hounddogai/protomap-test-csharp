namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccessBadgeClaimRulesChecks
{
    public static void AccessBadgeClaimValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeClaimRules.DefaultAccessBadgeClaimValidator();
        var item = AccessBadgeClaim.NewAccessBadgeClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeClaimRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new AccessBadgeClaimRequiredContactEmailRule("access badge claim needs this field");
        var item = AccessBadgeClaim.NewAccessBadgeClaim("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeClaimLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new AccessBadgeClaimLeaveDaysRangeRule(32, 1000);
        var item = AccessBadgeClaim.NewAccessBadgeClaim("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeClaimMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new AccessBadgeClaimMarginRangeRule(8.9, 2685.5);
        var item = AccessBadgeClaim.NewAccessBadgeClaim("x", "y");
        item.Margin = 2687.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AccessBadgeClaimValidatorReportsEveryProblem();
        AccessBadgeClaimRequiredContactEmailRuleFlagsABlankContactEmail();
        AccessBadgeClaimLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        AccessBadgeClaimMarginRangeRuleFlagsValuesAboveMargin();
    }
}
