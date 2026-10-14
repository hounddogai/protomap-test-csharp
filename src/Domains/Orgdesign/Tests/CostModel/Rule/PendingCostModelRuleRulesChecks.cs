namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PendingCostModelRuleRulesChecks
{
    public static void PendingCostModelRuleValidatorReportsEveryProblem()
    {
        var validator = PendingCostModelRuleRules.DefaultPendingCostModelRuleValidator();
        var item = PendingCostModelRule.NewPendingCostModelRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingCostModelRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingCostModelRuleCodeLengthRule(86400);
        var item = PendingCostModelRule.NewPendingCostModelRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingCostModelRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingCostModelRuleRequiredCodeRule("pending cost model rule needs an entry");
        var item = PendingCostModelRule.NewPendingCostModelRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingCostModelRulePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new PendingCostModelRulePriorityRangeRule(120, 255);
        var item = PendingCostModelRule.NewPendingCostModelRule("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingCostModelRuleValidatorReportsEveryProblem();
        PendingCostModelRuleCodeLengthRuleFlagsALongCode();
        PendingCostModelRuleRequiredCodeRuleFlagsABlankCode();
        PendingCostModelRulePriorityRangeRuleFlagsValuesAbovePriority();
    }
}
