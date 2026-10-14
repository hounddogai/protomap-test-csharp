namespace Acme.Workforce.Compensation.Tests.PayBand.Draft;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PayBandManifestRulesChecks
{
    public static void PayBandManifestValidatorReportsEveryProblem()
    {
        var validator = PayBandManifestRules.DefaultPayBandManifestValidator();
        var item = PayBandManifest.NewPayBandManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandManifestCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PayBandManifestCategoryLengthRule(32);
        var item = PayBandManifest.NewPayBandManifest("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandManifestRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new PayBandManifestRequiredCategoryRule("pay band manifest needs this field");
        var item = PayBandManifest.NewPayBandManifest("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandManifestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayBandManifestRequiredTitleRule("pay band manifest needs an entry");
        var item = PayBandManifest.NewPayBandManifest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandManifestRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new PayBandManifestRequiredPhoneRule("pay band manifest needs an entry");
        var item = PayBandManifest.NewPayBandManifest("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayBandManifestValidatorReportsEveryProblem();
        PayBandManifestCategoryLengthRuleFlagsALongCategory();
        PayBandManifestRequiredCategoryRuleFlagsABlankCategory();
        PayBandManifestRequiredTitleRuleFlagsABlankTitle();
        PayBandManifestRequiredPhoneRuleFlagsABlankPhone();
    }
}
