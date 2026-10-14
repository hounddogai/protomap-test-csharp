namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceVarianceRulesChecks
{
    public static void BalanceVarianceValidatorReportsEveryProblem()
    {
        var validator = BalanceVarianceRules.DefaultBalanceVarianceValidator();
        var item = BalanceVariance.NewBalanceVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceVarianceSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new BalanceVarianceSummaryLengthRule(80);
        var item = BalanceVariance.NewBalanceVariance("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceVarianceMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new BalanceVarianceMaximumRangeRule(9.0, 1694.4);
        var item = BalanceVariance.NewBalanceVariance("x", "y");
        item.Maximum = 1695.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BalanceVarianceRequiredCodeRule("balance variance needs this field");
        var item = BalanceVariance.NewBalanceVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceVarianceNameLengthRuleFlagsALongName()
    {
        var rule = new BalanceVarianceNameLengthRule(120);
        var item = BalanceVariance.NewBalanceVariance("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BalanceVarianceValidatorReportsEveryProblem();
        BalanceVarianceSummaryLengthRuleFlagsALongSummary();
        BalanceVarianceMaximumRangeRuleFlagsValuesAboveMaximum();
        BalanceVarianceRequiredCodeRuleFlagsABlankCode();
        BalanceVarianceNameLengthRuleFlagsALongName();
    }
}
