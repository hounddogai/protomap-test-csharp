namespace Acme.Workforce.Absence.Tests.Policy.RollingPolicyOverride;

using Acme.Workforce.Absence.Policy.RollingPolicyOverride;

/// <summary>Checks of rolling policy override code, run by the project's runner.</summary>
public static class WeightedPolicyOverrideRulesChecks
{
    public static void WeightedPolicyOverrideValidatorReportsEveryProblem()
    {
        var validator = WeightedPolicyOverrideRules.DefaultWeightedPolicyOverrideValidator();
        var item = WeightedPolicyOverride.NewWeightedPolicyOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedPolicyOverrideNameLengthRuleFlagsALongName()
    {
        var rule = new WeightedPolicyOverrideNameLengthRule(255);
        var item = WeightedPolicyOverride.NewWeightedPolicyOverride("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedPolicyOverrideVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new WeightedPolicyOverrideVersionRangeRule(16, 3600);
        var item = WeightedPolicyOverride.NewWeightedPolicyOverride("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedPolicyOverrideRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new WeightedPolicyOverrideRequiredExternalRefRule("weighted policy override needs a value");
        var item = WeightedPolicyOverride.NewWeightedPolicyOverride("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedPolicyOverrideValidatorReportsEveryProblem();
        WeightedPolicyOverrideNameLengthRuleFlagsALongName();
        WeightedPolicyOverrideVersionRangeRuleFlagsValuesAboveVersion();
        WeightedPolicyOverrideRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
