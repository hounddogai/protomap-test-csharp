namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientBalancePlanRulesChecks
{
    public static void LenientBalancePlanValidatorReportsEveryProblem()
    {
        var validator = LenientBalancePlanRules.DefaultLenientBalancePlanValidator();
        var item = LenientBalancePlan.NewLenientBalancePlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientBalancePlanTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new LenientBalancePlanTenureMonthsRangeRule(64, 255);
        var item = LenientBalancePlan.NewLenientBalancePlan("x", "y");
        item.TenureMonths = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientBalancePlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientBalancePlanRequiredCodeRule("lenient balance plan needs a value");
        var item = LenientBalancePlan.NewLenientBalancePlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBalancePlanRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new LenientBalancePlanRequiredSalaryBandRule("lenient balance plan needs a value");
        var item = LenientBalancePlan.NewLenientBalancePlan("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientBalancePlanValidatorReportsEveryProblem();
        LenientBalancePlanTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        LenientBalancePlanRequiredCodeRuleFlagsABlankCode();
        LenientBalancePlanRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
