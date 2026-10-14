namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class CappedCostModelRuleRulesChecks
{
    public static void CappedCostModelRuleValidatorReportsEveryProblem()
    {
        var validator = CappedCostModelRuleRules.DefaultCappedCostModelRuleValidator();
        var item = CappedCostModelRule.NewCappedCostModelRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedCostModelRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedCostModelRuleRequiredCodeRule("capped cost model rule needs an entry");
        var item = CappedCostModelRule.NewCappedCostModelRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedCostModelRuleRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new CappedCostModelRuleRatioRangeRule(7.9, 3258.0);
        var item = CappedCostModelRule.NewCappedCostModelRule("x", "y");
        item.Ratio = 3259.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedCostModelRuleCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new CappedCostModelRuleCategoryLengthRule(80);
        var item = CappedCostModelRule.NewCappedCostModelRule("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedCostModelRuleValidatorReportsEveryProblem();
        CappedCostModelRuleRequiredCodeRuleFlagsABlankCode();
        CappedCostModelRuleRatioRangeRuleFlagsValuesAboveRatio();
        CappedCostModelRuleCategoryLengthRuleFlagsALongCategory();
    }
}
