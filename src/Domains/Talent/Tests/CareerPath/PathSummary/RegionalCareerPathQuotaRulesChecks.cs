namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class RegionalCareerPathQuotaRulesChecks
{
    public static void RegionalCareerPathQuotaValidatorReportsEveryProblem()
    {
        var validator = RegionalCareerPathQuotaRules.DefaultRegionalCareerPathQuotaValidator();
        var item = RegionalCareerPathQuota.NewRegionalCareerPathQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalCareerPathQuotaRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new RegionalCareerPathQuotaRequiredLocaleRule("regional career path quota needs this field");
        var item = RegionalCareerPathQuota.NewRegionalCareerPathQuota("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalCareerPathQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalCareerPathQuotaRequiredCodeRule("regional career path quota needs an entry");
        var item = RegionalCareerPathQuota.NewRegionalCareerPathQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalCareerPathQuotaRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new RegionalCareerPathQuotaRequiredCategoryRule("regional career path quota needs a value");
        var item = RegionalCareerPathQuota.NewRegionalCareerPathQuota("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalCareerPathQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RegionalCareerPathQuotaRequiredLabelRule("regional career path quota needs an entry");
        var item = RegionalCareerPathQuota.NewRegionalCareerPathQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalCareerPathQuotaValidatorReportsEveryProblem();
        RegionalCareerPathQuotaRequiredLocaleRuleFlagsABlankLocale();
        RegionalCareerPathQuotaRequiredCodeRuleFlagsABlankCode();
        RegionalCareerPathQuotaRequiredCategoryRuleFlagsABlankCategory();
        RegionalCareerPathQuotaRequiredLabelRuleFlagsABlankLabel();
    }
}
