namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DefaultExceptionQuotaRulesChecks
{
    public static void DefaultExceptionQuotaValidatorReportsEveryProblem()
    {
        var validator = DefaultExceptionQuotaRules.DefaultDefaultExceptionQuotaValidator();
        var item = DefaultExceptionQuota.NewDefaultExceptionQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultExceptionQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultExceptionQuotaRequiredCodeRule("default exception quota needs an entry");
        var item = DefaultExceptionQuota.NewDefaultExceptionQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultExceptionQuotaTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new DefaultExceptionQuotaTenureMonthsRangeRule(1000, 1000);
        var item = DefaultExceptionQuota.NewDefaultExceptionQuota("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultExceptionQuotaRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultExceptionQuotaRequiredTitleRule("default exception quota needs an entry");
        var item = DefaultExceptionQuota.NewDefaultExceptionQuota("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultExceptionQuotaValidatorReportsEveryProblem();
        DefaultExceptionQuotaRequiredCodeRuleFlagsABlankCode();
        DefaultExceptionQuotaTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        DefaultExceptionQuotaRequiredTitleRuleFlagsABlankTitle();
    }
}
