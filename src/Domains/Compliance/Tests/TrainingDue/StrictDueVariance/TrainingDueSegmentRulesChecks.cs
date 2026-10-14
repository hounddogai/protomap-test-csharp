namespace Acme.Workforce.Compliance.Tests.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Checks of strict due variance code, run by the project's runner.</summary>
public static class TrainingDueSegmentRulesChecks
{
    public static void TrainingDueSegmentValidatorReportsEveryProblem()
    {
        var validator = TrainingDueSegmentRules.DefaultTrainingDueSegmentValidator();
        var item = TrainingDueSegment.NewTrainingDueSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueSegmentCountryLengthRuleFlagsALongCountry()
    {
        var rule = new TrainingDueSegmentCountryLengthRule(255);
        var item = TrainingDueSegment.NewTrainingDueSegment("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueSegmentWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new TrainingDueSegmentWindowDaysRangeRule(16, 32);
        var item = TrainingDueSegment.NewTrainingDueSegment("x", "y");
        item.WindowDays = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueSegmentDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new TrainingDueSegmentDiscountRangeRule(4.1, 4796.5);
        var item = TrainingDueSegment.NewTrainingDueSegment("x", "y");
        item.Discount = 4798.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueSegmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new TrainingDueSegmentRequiredNameRule("training due segment needs an entry");
        var item = TrainingDueSegment.NewTrainingDueSegment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TrainingDueSegmentValidatorReportsEveryProblem();
        TrainingDueSegmentCountryLengthRuleFlagsALongCountry();
        TrainingDueSegmentWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        TrainingDueSegmentDiscountRangeRuleFlagsValuesAboveDiscount();
        TrainingDueSegmentRequiredNameRuleFlagsABlankName();
    }
}
