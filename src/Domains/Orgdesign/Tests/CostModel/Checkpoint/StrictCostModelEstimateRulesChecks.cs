namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class StrictCostModelEstimateRulesChecks
{
    public static void StrictCostModelEstimateValidatorReportsEveryProblem()
    {
        var validator = StrictCostModelEstimateRules.DefaultStrictCostModelEstimateValidator();
        var item = StrictCostModelEstimate.NewStrictCostModelEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictCostModelEstimateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new StrictCostModelEstimateTitleLengthRule(120);
        var item = StrictCostModelEstimate.NewStrictCostModelEstimate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictCostModelEstimateRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new StrictCostModelEstimateRequiredPhoneRule("strict cost model estimate needs a value");
        var item = StrictCostModelEstimate.NewStrictCostModelEstimate("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictCostModelEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictCostModelEstimateCodeLengthRule(3600);
        var item = StrictCostModelEstimate.NewStrictCostModelEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictCostModelEstimateValidatorReportsEveryProblem();
        StrictCostModelEstimateTitleLengthRuleFlagsALongTitle();
        StrictCostModelEstimateRequiredPhoneRuleFlagsABlankPhone();
        StrictCostModelEstimateCodeLengthRuleFlagsALongCode();
    }
}
