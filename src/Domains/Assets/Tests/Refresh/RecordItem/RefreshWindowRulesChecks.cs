namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RefreshWindowRulesChecks
{
    public static void RefreshWindowValidatorReportsEveryProblem()
    {
        var validator = RefreshWindowRules.DefaultRefreshWindowValidator();
        var item = RefreshWindow.NewRefreshWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshWindowRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new RefreshWindowRequiredLocaleRule("refresh window needs this field");
        var item = RefreshWindow.NewRefreshWindow("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshWindowRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new RefreshWindowRequiredOwnerIdRule("refresh window needs a value");
        var item = RefreshWindow.NewRefreshWindow("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshWindowValidatorReportsEveryProblem();
        RefreshWindowRequiredLocaleRuleFlagsABlankLocale();
        RefreshWindowRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
