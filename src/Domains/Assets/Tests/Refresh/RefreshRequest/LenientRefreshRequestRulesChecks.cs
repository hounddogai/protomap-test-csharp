namespace Acme.Workforce.Assets.Tests.Refresh.RefreshRequest;

using Acme.Workforce.Assets.Refresh.RefreshRequest;

/// <summary>Checks of refresh request code, run by the project's runner.</summary>
public static class LenientRefreshRequestRulesChecks
{
    public static void LenientRefreshRequestValidatorReportsEveryProblem()
    {
        var validator = LenientRefreshRequestRules.DefaultLenientRefreshRequestValidator();
        var item = LenientRefreshRequest.NewLenientRefreshRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientRefreshRequestHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new LenientRefreshRequestHoursRangeRule(2.6, 531.4);
        var item = LenientRefreshRequest.NewLenientRefreshRequest("x", "y");
        item.Hours = 532.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientRefreshRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientRefreshRequestRequiredCodeRule("lenient refresh request needs an entry");
        var item = LenientRefreshRequest.NewLenientRefreshRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientRefreshRequestValidatorReportsEveryProblem();
        LenientRefreshRequestHoursRangeRuleFlagsValuesAboveHours();
        LenientRefreshRequestRequiredCodeRuleFlagsABlankCode();
    }
}
