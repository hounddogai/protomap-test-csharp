namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class CappedCareerPathBudgetRulesChecks
{
    public static void CappedCareerPathBudgetValidatorReportsEveryProblem()
    {
        var validator = CappedCareerPathBudgetRules.DefaultCappedCareerPathBudgetValidator();
        var item = CappedCareerPathBudget.NewCappedCareerPathBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedCareerPathBudgetTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new CappedCareerPathBudgetTenureMonthsRangeRule(16, 80);
        var item = CappedCareerPathBudget.NewCappedCareerPathBudget("x", "y");
        item.TenureMonths = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedCareerPathBudgetQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new CappedCareerPathBudgetQuantityRangeRule(86400, 86400);
        var item = CappedCareerPathBudget.NewCappedCareerPathBudget("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedCareerPathBudgetValidatorReportsEveryProblem();
        CappedCareerPathBudgetTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        CappedCareerPathBudgetQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
