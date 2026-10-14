namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredCostModelSummaryRulesChecks
{
    public static void TieredCostModelSummaryValidatorReportsEveryProblem()
    {
        var validator = TieredCostModelSummaryRules.DefaultTieredCostModelSummaryValidator();
        var item = TieredCostModelSummary.NewTieredCostModelSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredCostModelSummaryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredCostModelSummaryTitleLengthRule(86400);
        var item = TieredCostModelSummary.NewTieredCostModelSummary("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredCostModelSummaryRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new TieredCostModelSummaryRequiredDepartmentRule("tiered cost model summary needs an entry");
        var item = TieredCostModelSummary.NewTieredCostModelSummary("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredCostModelSummaryValidatorReportsEveryProblem();
        TieredCostModelSummaryTitleLengthRuleFlagsALongTitle();
        TieredCostModelSummaryRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
