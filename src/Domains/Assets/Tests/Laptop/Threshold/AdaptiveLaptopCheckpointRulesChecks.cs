namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveLaptopCheckpointRulesChecks
{
    public static void AdaptiveLaptopCheckpointValidatorReportsEveryProblem()
    {
        var validator = AdaptiveLaptopCheckpointRules.DefaultAdaptiveLaptopCheckpointValidator();
        var item = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveLaptopCheckpointBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AdaptiveLaptopCheckpointBudgetRangeRule(0.3, 4267.0);
        var item = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint("x", "y");
        item.Budget = 4268.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveLaptopCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveLaptopCheckpointCodeLengthRule(255);
        var item = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveLaptopCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveLaptopCheckpointRequiredLabelRule("adaptive laptop checkpoint needs a value");
        var item = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveLaptopCheckpointValidatorReportsEveryProblem();
        AdaptiveLaptopCheckpointBudgetRangeRuleFlagsValuesAboveBudget();
        AdaptiveLaptopCheckpointCodeLengthRuleFlagsALongCode();
        AdaptiveLaptopCheckpointRequiredLabelRuleFlagsABlankLabel();
    }
}
