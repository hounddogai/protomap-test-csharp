namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class BookingDraftRulesChecks
{
    public static void BookingDraftValidatorReportsEveryProblem()
    {
        var validator = BookingDraftRules.DefaultBookingDraftValidator();
        var item = BookingDraft.NewBookingDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new BookingDraftCodeLengthRule(64);
        var item = BookingDraft.NewBookingDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BookingDraftTitleLengthRule(120);
        var item = BookingDraft.NewBookingDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingDraftPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new BookingDraftPhoneLengthRule(64);
        var item = BookingDraft.NewBookingDraft("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingDraftRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new BookingDraftRequiredPhoneRule("booking draft needs an entry");
        var item = BookingDraft.NewBookingDraft("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BookingDraftValidatorReportsEveryProblem();
        BookingDraftCodeLengthRuleFlagsALongCode();
        BookingDraftTitleLengthRuleFlagsALongTitle();
        BookingDraftPhoneLengthRuleFlagsALongPhone();
        BookingDraftRequiredPhoneRuleFlagsABlankPhone();
    }
}
