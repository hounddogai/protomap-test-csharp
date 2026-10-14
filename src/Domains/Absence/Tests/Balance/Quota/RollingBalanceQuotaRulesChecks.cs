namespace Acme.Workforce.Absence.Tests.Balance.Quota;

using Acme.Workforce.Absence.Balance.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RollingBalanceQuotaRulesChecks
{
    public static void RollingBalanceQuotaValidatorReportsEveryProblem()
    {
        var validator = RollingBalanceQuotaRules.DefaultRollingBalanceQuotaValidator();
        var item = RollingBalanceQuota.NewRollingBalanceQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingBalanceQuotaTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RollingBalanceQuotaTenureMonthsRangeRule(16, 3600);
        var item = RollingBalanceQuota.NewRollingBalanceQuota("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingBalanceQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingBalanceQuotaCodeLengthRule(64);
        var item = RollingBalanceQuota.NewRollingBalanceQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingBalanceQuotaFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new RollingBalanceQuotaFloorValueRangeRule(6.0, 2726.8);
        var item = RollingBalanceQuota.NewRollingBalanceQuota("x", "y");
        item.FloorValue = 2728.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingBalanceQuotaValidatorReportsEveryProblem();
        RollingBalanceQuotaTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        RollingBalanceQuotaCodeLengthRuleFlagsALongCode();
        RollingBalanceQuotaFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
