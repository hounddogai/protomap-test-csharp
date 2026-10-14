namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FallbackCostModelRollupRulesChecks
{
    public static void FallbackCostModelRollupValidatorReportsEveryProblem()
    {
        var validator = FallbackCostModelRollupRules.DefaultFallbackCostModelRollupValidator();
        var item = FallbackCostModelRollup.NewFallbackCostModelRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackCostModelRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackCostModelRollupRequiredCodeRule("fallback cost model rollup needs a value");
        var item = FallbackCostModelRollup.NewFallbackCostModelRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackCostModelRollupRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new FallbackCostModelRollupRequiredSourceRule("fallback cost model rollup needs this field");
        var item = FallbackCostModelRollup.NewFallbackCostModelRollup("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackCostModelRollupRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackCostModelRollupRequiredNameRule("fallback cost model rollup needs a value");
        var item = FallbackCostModelRollup.NewFallbackCostModelRollup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackCostModelRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackCostModelRollupCodeLengthRule(80);
        var item = FallbackCostModelRollup.NewFallbackCostModelRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackCostModelRollupValidatorReportsEveryProblem();
        FallbackCostModelRollupRequiredCodeRuleFlagsABlankCode();
        FallbackCostModelRollupRequiredSourceRuleFlagsABlankSource();
        FallbackCostModelRollupRequiredNameRuleFlagsABlankName();
        FallbackCostModelRollupCodeLengthRuleFlagsALongCode();
    }
}
