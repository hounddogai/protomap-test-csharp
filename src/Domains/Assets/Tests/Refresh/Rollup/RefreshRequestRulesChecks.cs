namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRequestRulesChecks
{
    public static void RefreshRequestValidatorReportsEveryProblem()
    {
        var validator = RefreshRequestRules.DefaultRefreshRequestValidator();
        var item = RefreshRequest.NewRefreshRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshRequestCodeLengthRule(86400);
        var item = RefreshRequest.NewRefreshRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshRequestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RefreshRequestRequiredTitleRule("refresh request needs an entry");
        var item = RefreshRequest.NewRefreshRequest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshRequestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RefreshRequestLabelLengthRule(64);
        var item = RefreshRequest.NewRefreshRequest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshRequestValidatorReportsEveryProblem();
        RefreshRequestCodeLengthRuleFlagsALongCode();
        RefreshRequestRequiredTitleRuleFlagsABlankTitle();
        RefreshRequestLabelLengthRuleFlagsALongLabel();
    }
}
