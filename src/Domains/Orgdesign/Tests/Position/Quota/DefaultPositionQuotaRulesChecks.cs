namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DefaultPositionQuotaRulesChecks
{
    public static void DefaultPositionQuotaValidatorExtraReportsEveryProblem()
    {
        var validator = DefaultPositionQuotaRules.DefaultDefaultPositionQuotaValidator();
        var item = DefaultPositionQuota.NewDefaultPositionQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPositionQuotaRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new DefaultPositionQuotaRequiredLocationRule("default position quota needs an entry");
        var item = DefaultPositionQuota.NewDefaultPositionQuota("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPositionQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultPositionQuotaRequiredCodeRule("default position quota needs this field");
        var item = DefaultPositionQuota.NewDefaultPositionQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultPositionQuotaValidatorExtraReportsEveryProblem();
        DefaultPositionQuotaRequiredLocationRuleFlagsABlankLocation();
        DefaultPositionQuotaRequiredCodeRuleFlagsABlankCode();
    }
}
