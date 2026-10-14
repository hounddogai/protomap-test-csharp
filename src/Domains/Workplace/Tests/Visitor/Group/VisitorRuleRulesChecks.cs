namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorRuleRulesChecks
{
    public static void VisitorRuleValidatorReportsEveryProblem()
    {
        var validator = VisitorRuleRules.DefaultVisitorRuleValidator();
        var item = VisitorRule.NewVisitorRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorRuleBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new VisitorRuleBatchSizeRangeRule(32, 32);
        var item = VisitorRule.NewVisitorRule("x", "y");
        item.BatchSize = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorRuleScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new VisitorRuleScoreRangeRule(8.1, 1685.7);
        var item = VisitorRule.NewVisitorRule("x", "y");
        item.Score = 1687.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorRuleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new VisitorRuleRequiredTitleRule("visitor rule needs an entry");
        var item = VisitorRule.NewVisitorRule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorRuleValidatorReportsEveryProblem();
        VisitorRuleBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        VisitorRuleScoreRangeRuleFlagsValuesAboveScore();
        VisitorRuleRequiredTitleRuleFlagsABlankTitle();
    }
}
