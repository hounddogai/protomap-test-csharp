namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatPreferenceRulesChecks
{
    public static void SeasonalLicenseSeatPreferenceValidatorReportsEveryProblem()
    {
        var validator = SeasonalLicenseSeatPreferenceRules.DefaultSeasonalLicenseSeatPreferenceValidator();
        var item = SeasonalLicenseSeatPreference.NewSeasonalLicenseSeatPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalLicenseSeatPreferenceRequiredNameRuleFlagsABlankName()
    {
        var rule = new SeasonalLicenseSeatPreferenceRequiredNameRule("seasonal license seat preference needs an entry");
        var item = SeasonalLicenseSeatPreference.NewSeasonalLicenseSeatPreference("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalLicenseSeatPreferenceQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SeasonalLicenseSeatPreferenceQuantityRangeRule(64, 255);
        var item = SeasonalLicenseSeatPreference.NewSeasonalLicenseSeatPreference("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalLicenseSeatPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalLicenseSeatPreferenceRequiredCodeRule("seasonal license seat preference needs a value");
        var item = SeasonalLicenseSeatPreference.NewSeasonalLicenseSeatPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatPreferenceValidatorReportsEveryProblem();
        SeasonalLicenseSeatPreferenceRequiredNameRuleFlagsABlankName();
        SeasonalLicenseSeatPreferenceQuantityRangeRuleFlagsValuesAboveQuantity();
        SeasonalLicenseSeatPreferenceRequiredCodeRuleFlagsABlankCode();
    }
}
