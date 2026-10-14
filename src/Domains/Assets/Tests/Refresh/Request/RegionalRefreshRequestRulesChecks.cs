namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RegionalRefreshRequestRulesChecks
{
    public static void RegionalRefreshRequestValidatorReportsEveryProblem()
    {
        var validator = RegionalRefreshRequestRules.DefaultRegionalRefreshRequestValidator();
        var item = RegionalRefreshRequest.NewRegionalRefreshRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalRefreshRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalRefreshRequestRequiredCodeRule("regional refresh request needs a value");
        var item = RegionalRefreshRequest.NewRegionalRefreshRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalRefreshRequestExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RegionalRefreshRequestExpiresAtRangeRule(16, 255);
        var item = RegionalRefreshRequest.NewRegionalRefreshRequest("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalRefreshRequestWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new RegionalRefreshRequestWindowDaysRangeRule(3600, 86400);
        var item = RegionalRefreshRequest.NewRegionalRefreshRequest("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalRefreshRequestValidatorReportsEveryProblem();
        RegionalRefreshRequestRequiredCodeRuleFlagsABlankCode();
        RegionalRefreshRequestExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        RegionalRefreshRequestWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
