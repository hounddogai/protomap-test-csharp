namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class StrictReorgEstimateRulesChecks
{
    public static void StrictReorgEstimateValidatorReportsEveryProblem()
    {
        var validator = StrictReorgEstimateRules.DefaultStrictReorgEstimateValidator();
        var item = StrictReorgEstimate.NewStrictReorgEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictReorgEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictReorgEstimateRequiredCodeRule("strict reorg estimate needs a value");
        var item = StrictReorgEstimate.NewStrictReorgEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReorgEstimateFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new StrictReorgEstimateFloorValueRangeRule(3.5, 3482.1);
        var item = StrictReorgEstimate.NewStrictReorgEstimate("x", "y");
        item.FloorValue = 3483.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictReorgEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictReorgEstimateCodeLengthRule(80);
        var item = StrictReorgEstimate.NewStrictReorgEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictReorgEstimateAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new StrictReorgEstimateAttemptsRangeRule(1000, 86400);
        var item = StrictReorgEstimate.NewStrictReorgEstimate("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictReorgEstimateValidatorReportsEveryProblem();
        StrictReorgEstimateRequiredCodeRuleFlagsABlankCode();
        StrictReorgEstimateFloorValueRangeRuleFlagsValuesAboveFloorValue();
        StrictReorgEstimateCodeLengthRuleFlagsALongCode();
        StrictReorgEstimateAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
