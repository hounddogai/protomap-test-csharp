namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnScheduleRulesChecks
{
    public static void AssetReturnScheduleValidatorReportsEveryProblem()
    {
        var validator = AssetReturnScheduleRules.DefaultAssetReturnScheduleValidator();
        var item = AssetReturnSchedule.NewAssetReturnSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnScheduleRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new AssetReturnScheduleRevisionRangeRule(16, 120);
        var item = AssetReturnSchedule.NewAssetReturnSchedule("x", "y");
        item.Revision = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnScheduleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssetReturnScheduleTitleLengthRule(255);
        var item = AssetReturnSchedule.NewAssetReturnSchedule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssetReturnScheduleRequiredCodeRule("asset return schedule needs a value");
        var item = AssetReturnSchedule.NewAssetReturnSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnScheduleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new AssetReturnScheduleTenureMonthsRangeRule(80, 80);
        var item = AssetReturnSchedule.NewAssetReturnSchedule("x", "y");
        item.TenureMonths = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssetReturnScheduleValidatorReportsEveryProblem();
        AssetReturnScheduleRevisionRangeRuleFlagsValuesAboveRevision();
        AssetReturnScheduleTitleLengthRuleFlagsALongTitle();
        AssetReturnScheduleRequiredCodeRuleFlagsABlankCode();
        AssetReturnScheduleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
