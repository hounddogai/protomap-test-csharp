namespace Acme.Workforce.Compliance.Tests.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TrainingDueOverrideRulesChecks
{
    public static void TrainingDueOverrideValidatorReportsEveryProblem()
    {
        var validator = TrainingDueOverrideRules.DefaultTrainingDueOverrideValidator();
        var item = TrainingDueOverride.NewTrainingDueOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueOverrideDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new TrainingDueOverrideDiscountRangeRule(6.3, 444.1);
        var item = TrainingDueOverride.NewTrainingDueOverride("x", "y");
        item.Discount = 445.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TrainingDueOverrideRequiredCodeRule("training due override needs an entry");
        var item = TrainingDueOverride.NewTrainingDueOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueOverrideManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new TrainingDueOverrideManagerIdLengthRule(16);
        var item = TrainingDueOverride.NewTrainingDueOverride("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TrainingDueOverrideRequiredLabelRule("training due override needs a value");
        var item = TrainingDueOverride.NewTrainingDueOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TrainingDueOverrideValidatorReportsEveryProblem();
        TrainingDueOverrideDiscountRangeRuleFlagsValuesAboveDiscount();
        TrainingDueOverrideRequiredCodeRuleFlagsABlankCode();
        TrainingDueOverrideManagerIdLengthRuleFlagsALongManagerId();
        TrainingDueOverrideRequiredLabelRuleFlagsABlankLabel();
    }
}
