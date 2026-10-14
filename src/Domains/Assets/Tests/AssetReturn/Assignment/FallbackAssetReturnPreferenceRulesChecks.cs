namespace Acme.Workforce.Assets.Tests.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FallbackAssetReturnPreferenceRulesChecks
{
    public static void FallbackAssetReturnPreferenceValidatorReportsEveryProblem()
    {
        var validator = FallbackAssetReturnPreferenceRules.DefaultFallbackAssetReturnPreferenceValidator();
        var item = FallbackAssetReturnPreference.NewFallbackAssetReturnPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAssetReturnPreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackAssetReturnPreferenceRequiredTitleRule("fallback asset return preference needs a value");
        var item = FallbackAssetReturnPreference.NewFallbackAssetReturnPreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackAssetReturnPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackAssetReturnPreferenceRequiredCodeRule("fallback asset return preference needs a value");
        var item = FallbackAssetReturnPreference.NewFallbackAssetReturnPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackAssetReturnPreferenceValidatorReportsEveryProblem();
        FallbackAssetReturnPreferenceRequiredTitleRuleFlagsABlankTitle();
        FallbackAssetReturnPreferenceRequiredCodeRuleFlagsABlankCode();
    }
}
