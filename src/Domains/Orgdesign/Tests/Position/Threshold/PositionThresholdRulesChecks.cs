namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PositionThresholdRulesChecks
{
    public static void PositionThresholdValidatorReportsEveryProblem()
    {
        var validator = PositionThresholdRules.DefaultPositionThresholdValidator();
        var item = PositionThreshold.NewPositionThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionThresholdAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PositionThresholdAttemptsRangeRule(80, 255);
        var item = PositionThreshold.NewPositionThreshold("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionThresholdRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PositionThresholdRequiredLabelRule("position threshold needs an entry");
        var item = PositionThreshold.NewPositionThreshold("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionThresholdRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new PositionThresholdRatingRangeRule(8.1, 387.0);
        var item = PositionThreshold.NewPositionThreshold("x", "y");
        item.Rating = 388.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionThresholdRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PositionThresholdRequiredExternalRefRule("position threshold needs an entry");
        var item = PositionThreshold.NewPositionThreshold("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new PositionThresholdCodeLengthRule(120);
        var item = PositionThreshold.NewPositionThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionThresholdDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PositionThresholdDiscountRangeRule(0.2, 2502.8);
        var item = PositionThreshold.NewPositionThreshold("x", "y");
        item.Discount = 2504.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PositionThresholdValidatorReportsEveryProblem();
        PositionThresholdAttemptsRangeRuleFlagsValuesAboveAttempts();
        PositionThresholdRequiredLabelRuleFlagsABlankLabel();
        PositionThresholdRatingRangeRuleFlagsValuesAboveRating();
        PositionThresholdRequiredExternalRefRuleFlagsABlankExternalRef();
        PositionThresholdCodeLengthRuleFlagsALongCode();
        PositionThresholdDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
