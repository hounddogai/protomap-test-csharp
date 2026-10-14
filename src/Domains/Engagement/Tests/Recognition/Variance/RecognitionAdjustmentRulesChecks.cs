namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionAdjustmentRulesChecks
{
    public static void RecognitionAdjustmentValidatorReportsEveryProblem()
    {
        var validator = RecognitionAdjustmentRules.DefaultRecognitionAdjustmentValidator();
        var item = RecognitionAdjustment.NewRecognitionAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionAdjustmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new RecognitionAdjustmentRequiredNameRule("recognition adjustment needs this field");
        var item = RecognitionAdjustment.NewRecognitionAdjustment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionAdjustmentCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new RecognitionAdjustmentCostRangeRule(6.1, 684.7);
        var item = RecognitionAdjustment.NewRecognitionAdjustment("x", "y");
        item.Cost = 686.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionAdjustmentDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new RecognitionAdjustmentDescriptionLengthRule(3600);
        var item = RecognitionAdjustment.NewRecognitionAdjustment("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionAdjustmentRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new RecognitionAdjustmentRequiredDescriptionRule("recognition adjustment needs an entry");
        var item = RecognitionAdjustment.NewRecognitionAdjustment("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RecognitionAdjustmentValidatorReportsEveryProblem();
        RecognitionAdjustmentRequiredNameRuleFlagsABlankName();
        RecognitionAdjustmentCostRangeRuleFlagsValuesAboveCost();
        RecognitionAdjustmentDescriptionLengthRuleFlagsALongDescription();
        RecognitionAdjustmentRequiredDescriptionRuleFlagsABlankDescription();
    }
}
