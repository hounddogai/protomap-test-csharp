namespace Acme.Workforce.Orgdesign.Tests.Headcount.Quota;

using Acme.Workforce.Orgdesign.Headcount.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DefaultHeadcountQuotaRulesChecks
{
    public static void DefaultHeadcountQuotaValidatorReportsEveryProblem()
    {
        var validator = DefaultHeadcountQuotaRules.DefaultDefaultHeadcountQuotaValidator();
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultHeadcountQuotaGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new DefaultHeadcountQuotaGraceDaysRangeRule(32, 80);
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("x", "y");
        item.GraceDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultHeadcountQuotaRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultHeadcountQuotaRequiredNameRule("default headcount quota needs a value");
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultHeadcountQuotaRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new DefaultHeadcountQuotaRatioRangeRule(6.2, 3496.8);
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("x", "y");
        item.Ratio = 3498.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultHeadcountQuotaCountryLengthRuleFlagsALongCountry()
    {
        var rule = new DefaultHeadcountQuotaCountryLengthRule(3600);
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultHeadcountQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultHeadcountQuotaCodeLengthRule(16);
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultHeadcountQuotaNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultHeadcountQuotaNameLengthRule(16);
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultHeadcountQuotaValidatorReportsEveryProblem();
        DefaultHeadcountQuotaGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        DefaultHeadcountQuotaRequiredNameRuleFlagsABlankName();
        DefaultHeadcountQuotaRatioRangeRuleFlagsValuesAboveRatio();
        DefaultHeadcountQuotaCountryLengthRuleFlagsALongCountry();
        DefaultHeadcountQuotaCodeLengthRuleFlagsALongCode();
        DefaultHeadcountQuotaNameLengthRuleFlagsALongName();
    }
}
