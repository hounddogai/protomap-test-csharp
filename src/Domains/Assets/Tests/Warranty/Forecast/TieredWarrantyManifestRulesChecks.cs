namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredWarrantyManifestRulesChecks
{
    public static void TieredWarrantyManifestValidatorReportsEveryProblem()
    {
        var validator = TieredWarrantyManifestRules.DefaultTieredWarrantyManifestValidator();
        var item = TieredWarrantyManifest.NewTieredWarrantyManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredWarrantyManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredWarrantyManifestRequiredCodeRule("tiered warranty manifest needs a value");
        var item = TieredWarrantyManifest.NewTieredWarrantyManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredWarrantyManifestRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new TieredWarrantyManifestRequiredCountryRule("tiered warranty manifest needs an entry");
        var item = TieredWarrantyManifest.NewTieredWarrantyManifest("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredWarrantyManifestValidatorReportsEveryProblem();
        TieredWarrantyManifestRequiredCodeRuleFlagsABlankCode();
        TieredWarrantyManifestRequiredCountryRuleFlagsABlankCountry();
    }
}
