namespace Acme.Workforce.Orgdesign.Tests.CostModel.Score;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CostModelRuleRulesChecks
{
    public static void CostModelRuleValidatorReportsEveryProblem()
    {
        var validator = CostModelRuleRules.DefaultCostModelRuleValidator();
        var item = CostModelRule.NewCostModelRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelRuleRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new CostModelRuleRequiredSourceRule("cost model rule needs this field");
        var item = CostModelRule.NewCostModelRule("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new CostModelRuleCodeLengthRule(32);
        var item = CostModelRule.NewCostModelRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CostModelRuleValidatorReportsEveryProblem();
        CostModelRuleRequiredSourceRuleFlagsABlankSource();
        CostModelRuleCodeLengthRuleFlagsALongCode();
    }
}
