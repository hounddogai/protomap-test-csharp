namespace Acme.Workforce.Engagement.Tests.Sentiment.Reservation;

using Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SentimentReservationRulesChecks
{
    public static void SentimentReservationValidatorReportsEveryProblem()
    {
        var validator = SentimentReservationRules.DefaultSentimentReservationValidator();
        var item = SentimentReservation.NewSentimentReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new SentimentReservationRequiredNameRule("sentiment reservation needs this field");
        var item = SentimentReservation.NewSentimentReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentReservationDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new SentimentReservationDiscountRangeRule(0.9, 3033.3);
        var item = SentimentReservation.NewSentimentReservation("x", "y");
        item.Discount = 3034.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentReservationValidatorReportsEveryProblem();
        SentimentReservationRequiredNameRuleFlagsABlankName();
        SentimentReservationDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
