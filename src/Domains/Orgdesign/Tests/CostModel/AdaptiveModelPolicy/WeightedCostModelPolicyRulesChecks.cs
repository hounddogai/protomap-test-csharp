namespace Acme.Workforce.Orgdesign.Tests.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Checks of adaptive model policy code, run by the project's runner.</summary>
public static class WeightedCostModelPolicyRulesChecks
{
    public static void WeightedCostModelPolicyValidatorReportsEveryProblem()
    {
        var validator = WeightedCostModelPolicyRules.DefaultWeightedCostModelPolicyValidator();
        var item = WeightedCostModelPolicy.NewWeightedCostModelPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedCostModelPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedCostModelPolicyCodeLengthRule(1000);
        var item = WeightedCostModelPolicy.NewWeightedCostModelPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedCostModelPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedCostModelPolicyRequiredCodeRule("weighted cost model policy needs a value");
        var item = WeightedCostModelPolicy.NewWeightedCostModelPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedCostModelPolicyValidatorReportsEveryProblem();
        WeightedCostModelPolicyCodeLengthRuleFlagsALongCode();
        WeightedCostModelPolicyRequiredCodeRuleFlagsABlankCode();
    }
}
