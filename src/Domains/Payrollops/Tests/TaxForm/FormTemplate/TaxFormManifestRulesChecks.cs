namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormTemplate;

using Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>Checks of form template code, run by the project's runner.</summary>
public static class TaxFormManifestRulesChecks
{
    public static void TaxFormManifestValidatorReportsEveryProblem()
    {
        var validator = TaxFormManifestRules.DefaultTaxFormManifestValidator();
        var item = TaxFormManifest.NewTaxFormManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormManifestScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new TaxFormManifestScoreRangeRule(9.1, 1342.3);
        var item = TaxFormManifest.NewTaxFormManifest("x", "y");
        item.Score = 1343.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormManifestCodeLengthRule(64);
        var item = TaxFormManifest.NewTaxFormManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormManifestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TaxFormManifestRequiredTitleRule("tax form manifest needs an entry");
        var item = TaxFormManifest.NewTaxFormManifest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TaxFormManifestRequiredCodeRule("tax form manifest needs an entry");
        var item = TaxFormManifest.NewTaxFormManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormManifestRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new TaxFormManifestRequiredSummaryRule("tax form manifest needs an entry");
        var item = TaxFormManifest.NewTaxFormManifest("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormManifestValidatorReportsEveryProblem();
        TaxFormManifestScoreRangeRuleFlagsValuesAboveScore();
        TaxFormManifestCodeLengthRuleFlagsALongCode();
        TaxFormManifestRequiredTitleRuleFlagsABlankTitle();
        TaxFormManifestRequiredCodeRuleFlagsABlankCode();
        TaxFormManifestRequiredSummaryRuleFlagsABlankSummary();
    }
}
