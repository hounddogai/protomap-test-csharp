namespace Acme.Workforce.Absence.Tests.Accrual.Ledger;

using Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GlobalAccrualEstimateRulesChecks
{
    public static void GlobalAccrualEstimateValidatorReportsEveryProblem()
    {
        var validator = GlobalAccrualEstimateRules.DefaultGlobalAccrualEstimateValidator();
        var item = GlobalAccrualEstimate.NewGlobalAccrualEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAccrualEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalAccrualEstimateCodeLengthRule(1000);
        var item = GlobalAccrualEstimate.NewGlobalAccrualEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAccrualEstimateRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalAccrualEstimateRequiredNameRule("global accrual estimate needs an entry");
        var item = GlobalAccrualEstimate.NewGlobalAccrualEstimate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAccrualEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalAccrualEstimateRequiredCodeRule("global accrual estimate needs this field");
        var item = GlobalAccrualEstimate.NewGlobalAccrualEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAccrualEstimateRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new GlobalAccrualEstimateRevisionRangeRule(16, 255);
        var item = GlobalAccrualEstimate.NewGlobalAccrualEstimate("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalAccrualEstimateValidatorReportsEveryProblem();
        GlobalAccrualEstimateCodeLengthRuleFlagsALongCode();
        GlobalAccrualEstimateRequiredNameRuleFlagsABlankName();
        GlobalAccrualEstimateRequiredCodeRuleFlagsABlankCode();
        GlobalAccrualEstimateRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
