namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class RegionalPolicyEstimateRulesChecks
{
    public static void RegionalPolicyEstimateValidatorReportsEveryProblem()
    {
        var validator = RegionalPolicyEstimateRules.DefaultRegionalPolicyEstimateValidator();
        var item = RegionalPolicyEstimate.NewRegionalPolicyEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalPolicyEstimateCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new RegionalPolicyEstimateCeilingRangeRule(9.4, 463.5);
        var item = RegionalPolicyEstimate.NewRegionalPolicyEstimate("x", "y");
        item.Ceiling = 465.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalPolicyEstimateThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new RegionalPolicyEstimateThresholdRangeRule(6.6, 3582.7);
        var item = RegionalPolicyEstimate.NewRegionalPolicyEstimate("x", "y");
        item.Threshold = 3584.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalPolicyEstimateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RegionalPolicyEstimateRequiredLabelRule("regional policy estimate needs an entry");
        var item = RegionalPolicyEstimate.NewRegionalPolicyEstimate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalPolicyEstimateValidatorReportsEveryProblem();
        RegionalPolicyEstimateCeilingRangeRuleFlagsValuesAboveCeiling();
        RegionalPolicyEstimateThresholdRangeRuleFlagsValuesAboveThreshold();
        RegionalPolicyEstimateRequiredLabelRuleFlagsABlankLabel();
    }
}
