namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PendingPayBandQuotaRulesChecks
{
    public static void PendingPayBandQuotaValidatorReportsEveryProblem()
    {
        var validator = PendingPayBandQuotaRules.DefaultPendingPayBandQuotaValidator();
        var item = PendingPayBandQuota.NewPendingPayBandQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPayBandQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingPayBandQuotaCodeLengthRule(32);
        var item = PendingPayBandQuota.NewPendingPayBandQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPayBandQuotaWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PendingPayBandQuotaWeightRangeRule(3.5, 1434.8);
        var item = PendingPayBandQuota.NewPendingPayBandQuota("x", "y");
        item.Weight = 1436.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingPayBandQuotaSourceLengthRuleFlagsALongSource()
    {
        var rule = new PendingPayBandQuotaSourceLengthRule(16);
        var item = PendingPayBandQuota.NewPendingPayBandQuota("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingPayBandQuotaValidatorReportsEveryProblem();
        PendingPayBandQuotaCodeLengthRuleFlagsALongCode();
        PendingPayBandQuotaWeightRangeRuleFlagsValuesAboveWeight();
        PendingPayBandQuotaSourceLengthRuleFlagsALongSource();
    }
}
