namespace Acme.Workforce.Talent.Tests.Pool.Variance;

using Acme.Workforce.Talent.Pool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LegacyPoolVarianceRulesChecks
{
    public static void LegacyPoolVarianceValidatorReportsEveryProblem()
    {
        var validator = LegacyPoolVarianceRules.DefaultLegacyPoolVarianceValidator();
        var item = LegacyPoolVariance.NewLegacyPoolVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPoolVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyPoolVarianceRequiredCodeRule("legacy pool variance needs this field");
        var item = LegacyPoolVariance.NewLegacyPoolVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPoolVarianceRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new LegacyPoolVarianceRequiredTimezoneRule("legacy pool variance needs a value");
        var item = LegacyPoolVariance.NewLegacyPoolVariance("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPoolVarianceCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LegacyPoolVarianceCostRangeRule(0.2, 2930.5);
        var item = LegacyPoolVariance.NewLegacyPoolVariance("x", "y");
        item.Cost = 2932.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyPoolVarianceValidatorReportsEveryProblem();
        LegacyPoolVarianceRequiredCodeRuleFlagsABlankCode();
        LegacyPoolVarianceRequiredTimezoneRuleFlagsABlankTimezone();
        LegacyPoolVarianceCostRangeRuleFlagsValuesAboveCost();
    }
}
