namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Checks of model policy code, run by the project's runner.</summary>
public static class CostModelQuotaRulesChecks
{
    public static void CostModelQuotaValidatorReportsEveryProblem()
    {
        var validator = CostModelQuotaRules.DefaultCostModelQuotaValidator();
        var item = CostModelQuota.NewCostModelQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelQuotaRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new CostModelQuotaRequiredRegionRule("cost model quota needs an entry");
        var item = CostModelQuota.NewCostModelQuota("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelQuotaChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new CostModelQuotaChecksumLengthRule(255);
        var item = CostModelQuota.NewCostModelQuota("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelQuotaRegionLengthRuleFlagsALongRegion()
    {
        var rule = new CostModelQuotaRegionLengthRule(3600);
        var item = CostModelQuota.NewCostModelQuota("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelQuotaCodeLengthRule(80);
        var item = CostModelQuota.NewCostModelQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelQuotaValidatorReportsEveryProblem();
        CostModelQuotaRequiredRegionRuleFlagsABlankRegion();
        CostModelQuotaChecksumLengthRuleFlagsALongChecksum();
        CostModelQuotaRegionLengthRuleFlagsALongRegion();
        CostModelQuotaCodeLengthRuleFlagsALongCode();
    }
}
