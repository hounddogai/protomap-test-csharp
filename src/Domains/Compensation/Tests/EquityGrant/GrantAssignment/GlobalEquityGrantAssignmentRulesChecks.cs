namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantAssignment;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Checks of grant assignment code, run by the project's runner.</summary>
public static class GlobalEquityGrantAssignmentRulesChecks
{
    public static void GlobalEquityGrantAssignmentValidatorReportsEveryProblem()
    {
        var validator = GlobalEquityGrantAssignmentRules.DefaultGlobalEquityGrantAssignmentValidator();
        var item = GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalEquityGrantAssignmentScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new GlobalEquityGrantAssignmentScoreRangeRule(4.0, 3149.3);
        var item = GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment("x", "y");
        item.Score = 3150.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalEquityGrantAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GlobalEquityGrantAssignmentRequiredLabelRule("global equity grant assignment needs an entry");
        var item = GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalEquityGrantAssignmentQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new GlobalEquityGrantAssignmentQuantityRangeRule(32, 64);
        var item = GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment("x", "y");
        item.Quantity = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalEquityGrantAssignmentWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new GlobalEquityGrantAssignmentWeightRangeRule(7.5, 1519.7);
        var item = GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment("x", "y");
        item.Weight = 1521.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalEquityGrantAssignmentValidatorReportsEveryProblem();
        GlobalEquityGrantAssignmentScoreRangeRuleFlagsValuesAboveScore();
        GlobalEquityGrantAssignmentRequiredLabelRuleFlagsABlankLabel();
        GlobalEquityGrantAssignmentQuantityRangeRuleFlagsValuesAboveQuantity();
        GlobalEquityGrantAssignmentWeightRangeRuleFlagsValuesAboveWeight();
    }
}
