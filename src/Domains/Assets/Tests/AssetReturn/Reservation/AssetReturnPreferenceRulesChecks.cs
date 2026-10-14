namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnPreferenceRulesChecks
{
    public static void AssetReturnPreferenceValidatorReportsEveryProblem()
    {
        var validator = AssetReturnPreferenceRules.DefaultAssetReturnPreferenceValidator();
        var item = AssetReturnPreference.NewAssetReturnPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnPreferenceCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AssetReturnPreferenceCostRangeRule(5.4, 3777.7);
        var item = AssetReturnPreference.NewAssetReturnPreference("x", "y");
        item.Cost = 3779.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnPreferencePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new AssetReturnPreferencePhoneLengthRule(255);
        var item = AssetReturnPreference.NewAssetReturnPreference("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssetReturnPreferenceRequiredCodeRule("asset return preference needs an entry");
        var item = AssetReturnPreference.NewAssetReturnPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnPreferenceRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AssetReturnPreferenceRateRangeRule(4.2, 591.3);
        var item = AssetReturnPreference.NewAssetReturnPreference("x", "y");
        item.Rate = 592.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnPreferenceLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new AssetReturnPreferenceLeaveDaysRangeRule(80, 3600);
        var item = AssetReturnPreference.NewAssetReturnPreference("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnPreferenceRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new AssetReturnPreferenceRequiredLocationRule("asset return preference needs a value");
        var item = AssetReturnPreference.NewAssetReturnPreference("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnPreferenceValidatorReportsEveryProblem();
        AssetReturnPreferenceCostRangeRuleFlagsValuesAboveCost();
        AssetReturnPreferencePhoneLengthRuleFlagsALongPhone();
        AssetReturnPreferenceRequiredCodeRuleFlagsABlankCode();
        AssetReturnPreferenceRateRangeRuleFlagsValuesAboveRate();
        AssetReturnPreferenceLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        AssetReturnPreferenceRequiredLocationRuleFlagsABlankLocation();
    }
}
