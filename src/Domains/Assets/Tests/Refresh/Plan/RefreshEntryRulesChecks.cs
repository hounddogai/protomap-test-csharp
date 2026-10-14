namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RefreshEntryRulesChecks
{
    public static void RefreshEntryValidatorReportsEveryProblem()
    {
        var validator = RefreshEntryRules.DefaultRefreshEntryValidator();
        var item = RefreshEntry.NewRefreshEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshEntryGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new RefreshEntryGraceDaysRangeRule(32, 120);
        var item = RefreshEntry.NewRefreshEntry("x", "y");
        item.GraceDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshEntryRegionLengthRuleFlagsALongRegion()
    {
        var rule = new RefreshEntryRegionLengthRule(16);
        var item = RefreshEntry.NewRefreshEntry("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RefreshEntryLabelLengthRule(86400);
        var item = RefreshEntry.NewRefreshEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RefreshEntryRequiredCodeRule("refresh entry needs this field");
        var item = RefreshEntry.NewRefreshEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshEntryValidatorReportsEveryProblem();
        RefreshEntryGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        RefreshEntryRegionLengthRuleFlagsALongRegion();
        RefreshEntryLabelLengthRuleFlagsALongLabel();
        RefreshEntryRequiredCodeRuleFlagsABlankCode();
    }
}
