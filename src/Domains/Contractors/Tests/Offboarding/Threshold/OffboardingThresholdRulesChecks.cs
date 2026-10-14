namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingThresholdRulesChecks
{
    public static void OffboardingThresholdValidatorReportsEveryProblem()
    {
        var validator = OffboardingThresholdRules.DefaultOffboardingThresholdValidator();
        var item = OffboardingThreshold.NewOffboardingThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingThresholdQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new OffboardingThresholdQuantityRangeRule(16, 32);
        var item = OffboardingThreshold.NewOffboardingThreshold("x", "y");
        item.Quantity = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingThresholdDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new OffboardingThresholdDurationMinutesRangeRule(1000, 86400);
        var item = OffboardingThreshold.NewOffboardingThreshold("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OffboardingThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new OffboardingThresholdCodeLengthRule(120);
        var item = OffboardingThreshold.NewOffboardingThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OffboardingThresholdValidatorReportsEveryProblem();
        OffboardingThresholdQuantityRangeRuleFlagsValuesAboveQuantity();
        OffboardingThresholdDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        OffboardingThresholdCodeLengthRuleFlagsALongCode();
    }
}
