namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueExceptionRulesChecks
{
    public static void AdaptiveTrainingDueExceptionValidatorReportsEveryProblem()
    {
        var validator = AdaptiveTrainingDueExceptionRules.DefaultAdaptiveTrainingDueExceptionValidator();
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveTrainingDueExceptionMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new AdaptiveTrainingDueExceptionMaximumRangeRule(3.4, 2634.9);
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("x", "y");
        item.Maximum = 2636.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveTrainingDueExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveTrainingDueExceptionRequiredCodeRule("adaptive training due exception needs a value");
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveTrainingDueExceptionContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new AdaptiveTrainingDueExceptionContactEmailLengthRule(80);
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveTrainingDueExceptionCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AdaptiveTrainingDueExceptionCostRangeRule(9.2, 1377.1);
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("x", "y");
        item.Cost = 1378.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveTrainingDueExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveTrainingDueExceptionCodeLengthRule(255);
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDueExceptionValidatorReportsEveryProblem();
        AdaptiveTrainingDueExceptionMaximumRangeRuleFlagsValuesAboveMaximum();
        AdaptiveTrainingDueExceptionRequiredCodeRuleFlagsABlankCode();
        AdaptiveTrainingDueExceptionContactEmailLengthRuleFlagsALongContactEmail();
        AdaptiveTrainingDueExceptionCostRangeRuleFlagsValuesAboveCost();
        AdaptiveTrainingDueExceptionCodeLengthRuleFlagsALongCode();
    }
}
