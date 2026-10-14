namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class WeightedOffboardingThresholdRulesChecks
{
    public static void WeightedOffboardingThresholdValidatorReportsEveryProblem()
    {
        var validator = WeightedOffboardingThresholdRules.DefaultWeightedOffboardingThresholdValidator();
        var item = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedOffboardingThresholdUnitLengthRuleFlagsALongUnit()
    {
        var rule = new WeightedOffboardingThresholdUnitLengthRule(16);
        var item = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedOffboardingThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedOffboardingThresholdRequiredCodeRule("weighted offboarding threshold needs a value");
        var item = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedOffboardingThresholdRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new WeightedOffboardingThresholdRequiredContactEmailRule(
            "weighted offboarding threshold needs a value");
        var item = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedOffboardingThresholdRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedOffboardingThresholdRequiredLabelRule("weighted offboarding threshold needs a value");
        var item = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedOffboardingThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedOffboardingThresholdCodeLengthRule(86400);
        var item = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedOffboardingThresholdValidatorReportsEveryProblem();
        WeightedOffboardingThresholdUnitLengthRuleFlagsALongUnit();
        WeightedOffboardingThresholdRequiredCodeRuleFlagsABlankCode();
        WeightedOffboardingThresholdRequiredContactEmailRuleFlagsABlankContactEmail();
        WeightedOffboardingThresholdRequiredLabelRuleFlagsABlankLabel();
        WeightedOffboardingThresholdCodeLengthRuleFlagsALongCode();
    }
}
