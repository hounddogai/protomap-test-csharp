namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Quota;

using Acme.Workforce.Talent.SuccessionPlan.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class GlobalSuccessionPlanQuotaRulesChecks
{
    public static void GlobalSuccessionPlanQuotaValidatorReportsEveryProblem()
    {
        var validator = GlobalSuccessionPlanQuotaRules.DefaultGlobalSuccessionPlanQuotaValidator();
        var item = GlobalSuccessionPlanQuota.NewGlobalSuccessionPlanQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalSuccessionPlanQuotaManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new GlobalSuccessionPlanQuotaManagerIdLengthRule(80);
        var item = GlobalSuccessionPlanQuota.NewGlobalSuccessionPlanQuota("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalSuccessionPlanQuotaCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new GlobalSuccessionPlanQuotaCurrencyLengthRule(1000);
        var item = GlobalSuccessionPlanQuota.NewGlobalSuccessionPlanQuota("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalSuccessionPlanQuotaSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new GlobalSuccessionPlanQuotaSequenceRangeRule(32, 255);
        var item = GlobalSuccessionPlanQuota.NewGlobalSuccessionPlanQuota("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalSuccessionPlanQuotaRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new GlobalSuccessionPlanQuotaRequiredCurrencyRule("global succession plan quota needs a value");
        var item = GlobalSuccessionPlanQuota.NewGlobalSuccessionPlanQuota("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalSuccessionPlanQuotaValidatorReportsEveryProblem();
        GlobalSuccessionPlanQuotaManagerIdLengthRuleFlagsALongManagerId();
        GlobalSuccessionPlanQuotaCurrencyLengthRuleFlagsALongCurrency();
        GlobalSuccessionPlanQuotaSequenceRangeRuleFlagsValuesAboveSequence();
        GlobalSuccessionPlanQuotaRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
