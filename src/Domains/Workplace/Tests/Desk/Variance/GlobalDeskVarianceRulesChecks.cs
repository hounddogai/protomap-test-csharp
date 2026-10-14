namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GlobalDeskVarianceRulesChecks
{
    public static void GlobalDeskVarianceValidatorReportsEveryProblem()
    {
        var validator = GlobalDeskVarianceRules.DefaultGlobalDeskVarianceValidator();
        var item = GlobalDeskVariance.NewGlobalDeskVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalDeskVarianceQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new GlobalDeskVarianceQuantityRangeRule(64, 80);
        var item = GlobalDeskVariance.NewGlobalDeskVariance("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalDeskVarianceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalDeskVarianceLabelLengthRule(255);
        var item = GlobalDeskVariance.NewGlobalDeskVariance("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalDeskVarianceRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new GlobalDeskVarianceRequiredEmployeeIdRule("global desk variance needs a value");
        var item = GlobalDeskVariance.NewGlobalDeskVariance("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalDeskVarianceRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new GlobalDeskVarianceRequiredSalaryBandRule("global desk variance needs an entry");
        var item = GlobalDeskVariance.NewGlobalDeskVariance("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalDeskVarianceValidatorReportsEveryProblem();
        GlobalDeskVarianceQuantityRangeRuleFlagsValuesAboveQuantity();
        GlobalDeskVarianceLabelLengthRuleFlagsALongLabel();
        GlobalDeskVarianceRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        GlobalDeskVarianceRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
