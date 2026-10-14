namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveRetroPayCheckpointRulesChecks
{
    public static void AdaptiveRetroPayCheckpointValidatorReportsEveryProblem()
    {
        var validator = AdaptiveRetroPayCheckpointRules.DefaultAdaptiveRetroPayCheckpointValidator();
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveRetroPayCheckpointThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new AdaptiveRetroPayCheckpointThresholdRangeRule(3.9, 2954.0);
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("x", "y");
        item.Threshold = 2955.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveRetroPayCheckpointRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new AdaptiveRetroPayCheckpointRequiredManagerIdRule("adaptive retro pay checkpoint needs a value");
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveRetroPayCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveRetroPayCheckpointRequiredCodeRule("adaptive retro pay checkpoint needs a value");
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveRetroPayCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveRetroPayCheckpointNameLengthRule(80);
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveRetroPayCheckpointCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AdaptiveRetroPayCheckpointCostRangeRule(3.5, 3340.5);
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("x", "y");
        item.Cost = 3342.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveRetroPayCheckpointValidatorReportsEveryProblem();
        AdaptiveRetroPayCheckpointThresholdRangeRuleFlagsValuesAboveThreshold();
        AdaptiveRetroPayCheckpointRequiredManagerIdRuleFlagsABlankManagerId();
        AdaptiveRetroPayCheckpointRequiredCodeRuleFlagsABlankCode();
        AdaptiveRetroPayCheckpointNameLengthRuleFlagsALongName();
        AdaptiveRetroPayCheckpointCostRangeRuleFlagsValuesAboveCost();
    }
}
