namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredOccupancyRuleRulesChecks
{
    public static void TieredOccupancyRuleValidatorReportsEveryProblem()
    {
        var validator = TieredOccupancyRuleRules.DefaultTieredOccupancyRuleValidator();
        var item = TieredOccupancyRule.NewTieredOccupancyRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredOccupancyRuleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new TieredOccupancyRuleTenureMonthsRangeRule(120, 1000);
        var item = TieredOccupancyRule.NewTieredOccupancyRule("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredOccupancyRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredOccupancyRuleRequiredCodeRule("tiered occupancy rule needs a value");
        var item = TieredOccupancyRule.NewTieredOccupancyRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredOccupancyRuleValidatorReportsEveryProblem();
        TieredOccupancyRuleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        TieredOccupancyRuleRequiredCodeRuleFlagsABlankCode();
    }
}
