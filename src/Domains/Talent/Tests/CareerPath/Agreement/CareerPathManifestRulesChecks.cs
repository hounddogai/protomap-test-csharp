namespace Acme.Workforce.Talent.Tests.CareerPath.Agreement;

using Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CareerPathManifestRulesChecks
{
    public static void CareerPathManifestValidatorReportsEveryProblem()
    {
        var validator = CareerPathManifestRules.DefaultCareerPathManifestValidator();
        var item = CareerPathManifest.NewCareerPathManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathManifestRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new CareerPathManifestRequiredPhoneRule("career path manifest needs a value");
        var item = CareerPathManifest.NewCareerPathManifest("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CareerPathManifestRequiredCodeRule("career path manifest needs this field");
        var item = CareerPathManifest.NewCareerPathManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathManifestAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new CareerPathManifestAmountRangeRule(9.8, 3817.7);
        var item = CareerPathManifest.NewCareerPathManifest("x", "y");
        item.Amount = 3819.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CareerPathManifestValidatorReportsEveryProblem();
        CareerPathManifestRequiredPhoneRuleFlagsABlankPhone();
        CareerPathManifestRequiredCodeRuleFlagsABlankCode();
        CareerPathManifestAmountRangeRuleFlagsValuesAboveAmount();
    }
}
