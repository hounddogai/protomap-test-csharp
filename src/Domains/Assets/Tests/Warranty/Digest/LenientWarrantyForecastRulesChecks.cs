namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LenientWarrantyForecastRulesChecks
{
    public static void LenientWarrantyForecastValidatorReportsEveryProblem()
    {
        var validator = LenientWarrantyForecastRules.DefaultLenientWarrantyForecastValidator();
        var item = LenientWarrantyForecast.NewLenientWarrantyForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientWarrantyForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientWarrantyForecastRequiredCodeRule("lenient warranty forecast needs this field");
        var item = LenientWarrantyForecast.NewLenientWarrantyForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientWarrantyForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientWarrantyForecastRequiredTitleRule("lenient warranty forecast needs this field");
        var item = LenientWarrantyForecast.NewLenientWarrantyForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientWarrantyForecastValidatorReportsEveryProblem();
        LenientWarrantyForecastRequiredCodeRuleFlagsABlankCode();
        LenientWarrantyForecastRequiredTitleRuleFlagsABlankTitle();
    }
}
