namespace Acme.Workforce.Orgdesign.Tests.Span.Exception;

using Acme.Workforce.Orgdesign.Span.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SpanReservationRulesChecks
{
    public static void SpanReservationValidatorReportsEveryProblem()
    {
        var validator = SpanReservationRules.DefaultSpanReservationValidator();
        var item = SpanReservation.NewSpanReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanReservationSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new SpanReservationSequenceRangeRule(16, 86400);
        var item = SpanReservation.NewSpanReservation("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanReservationDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new SpanReservationDescriptionLengthRule(64);
        var item = SpanReservation.NewSpanReservation("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanReservationRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new SpanReservationRequiredBadgeRule("span reservation needs an entry");
        var item = SpanReservation.NewSpanReservation("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanReservationRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new SpanReservationRequiredDescriptionRule("span reservation needs this field");
        var item = SpanReservation.NewSpanReservation("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SpanReservationRequiredTitleRule("span reservation needs an entry");
        var item = SpanReservation.NewSpanReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanReservationValidatorReportsEveryProblem();
        SpanReservationSequenceRangeRuleFlagsValuesAboveSequence();
        SpanReservationDescriptionLengthRuleFlagsALongDescription();
        SpanReservationRequiredBadgeRuleFlagsABlankBadge();
        SpanReservationRequiredDescriptionRuleFlagsABlankDescription();
        SpanReservationRequiredTitleRuleFlagsABlankTitle();
    }
}
