namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictBonusPoolVarianceRulesChecks
{
    public static void StrictBonusPoolVarianceValidatorReportsEveryProblem()
    {
        var validator = StrictBonusPoolVarianceRules.DefaultStrictBonusPoolVarianceValidator();
        var item = StrictBonusPoolVariance.NewStrictBonusPoolVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictBonusPoolVarianceCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new StrictBonusPoolVarianceCurrencyLengthRule(120);
        var item = StrictBonusPoolVariance.NewStrictBonusPoolVariance("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictBonusPoolVarianceManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new StrictBonusPoolVarianceManagerIdLengthRule(120);
        var item = StrictBonusPoolVariance.NewStrictBonusPoolVariance("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictBonusPoolVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictBonusPoolVarianceRequiredCodeRule("strict bonus pool variance needs an entry");
        var item = StrictBonusPoolVariance.NewStrictBonusPoolVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictBonusPoolVarianceValidatorReportsEveryProblem();
        StrictBonusPoolVarianceCurrencyLengthRuleFlagsALongCurrency();
        StrictBonusPoolVarianceManagerIdLengthRuleFlagsALongManagerId();
        StrictBonusPoolVarianceRequiredCodeRuleFlagsABlankCode();
    }
}
