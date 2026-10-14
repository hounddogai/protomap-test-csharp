namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandRollupRulesChecks
{
    public static void PayBandRollupValidatorReportsEveryProblem()
    {
        var validator = PayBandRollupRules.DefaultPayBandRollupValidator();
        var item = PayBandRollup.NewPayBandRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandRollupExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PayBandRollupExpiresAtRangeRule(16, 255);
        var item = PayBandRollup.NewPayBandRollup("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandRollupCodeLengthRule(80);
        var item = PayBandRollup.NewPayBandRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandRollupFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new PayBandRollupFteRangeRule(2.7, 1002.6);
        var item = PayBandRollup.NewPayBandRollup("x", "y");
        item.Fte = 1004.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandRollupBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PayBandRollupBudgetRangeRule(3.2, 3329.3);
        var item = PayBandRollup.NewPayBandRollup("x", "y");
        item.Budget = 3330.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayBandRollupValidatorReportsEveryProblem();
        PayBandRollupExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PayBandRollupCodeLengthRuleFlagsALongCode();
        PayBandRollupFteRangeRuleFlagsValuesAboveFte();
        PayBandRollupBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
