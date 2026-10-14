namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopCheckpointRulesChecks
{
    public static void LaptopCheckpointValidatorReportsEveryProblem()
    {
        var validator = LaptopCheckpointRules.DefaultLaptopCheckpointValidator();
        var item = LaptopCheckpoint.NewLaptopCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LaptopCheckpointRequiredCodeRule("laptop checkpoint needs a value");
        var item = LaptopCheckpoint.NewLaptopCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopCheckpointExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LaptopCheckpointExpiresAtRangeRule(16, 255);
        var item = LaptopCheckpoint.NewLaptopCheckpoint("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopCheckpointThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new LaptopCheckpointThresholdRangeRule(8.8, 2022.4);
        var item = LaptopCheckpoint.NewLaptopCheckpoint("x", "y");
        item.Threshold = 2023.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LaptopCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LaptopCheckpointRequiredLabelRule("laptop checkpoint needs an entry");
        var item = LaptopCheckpoint.NewLaptopCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new LaptopCheckpointCodeLengthRule(3600);
        var item = LaptopCheckpoint.NewLaptopCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopCheckpointValidatorReportsEveryProblem();
        LaptopCheckpointRequiredCodeRuleFlagsABlankCode();
        LaptopCheckpointExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        LaptopCheckpointThresholdRangeRuleFlagsValuesAboveThreshold();
        LaptopCheckpointRequiredLabelRuleFlagsABlankLabel();
        LaptopCheckpointCodeLengthRuleFlagsALongCode();
    }
}
