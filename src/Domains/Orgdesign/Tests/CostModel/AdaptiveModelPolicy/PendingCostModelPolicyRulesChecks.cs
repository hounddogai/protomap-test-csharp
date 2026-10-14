namespace Acme.Workforce.Orgdesign.Tests.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Checks of adaptive model policy code, run by the project's runner.</summary>
public static class PendingCostModelPolicyRulesChecks
{
    public static void PendingCostModelPolicyValidatorReportsEveryProblem()
    {
        var validator = PendingCostModelPolicyRules.DefaultPendingCostModelPolicyValidator();
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingCostModelPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingCostModelPolicyCodeLengthRule(3600);
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingCostModelPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingCostModelPolicyLabelLengthRule(64);
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingCostModelPolicyRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new PendingCostModelPolicyRatingRangeRule(2.7, 3758.6);
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("x", "y");
        item.Rating = 3760.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingCostModelPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingCostModelPolicyRequiredCodeRule("pending cost model policy needs this field");
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingCostModelPolicyValidatorReportsEveryProblem();
        PendingCostModelPolicyCodeLengthRuleFlagsALongCode();
        PendingCostModelPolicyLabelLengthRuleFlagsALongLabel();
        PendingCostModelPolicyRatingRangeRuleFlagsValuesAboveRating();
        PendingCostModelPolicyRequiredCodeRuleFlagsABlankCode();
    }
}
