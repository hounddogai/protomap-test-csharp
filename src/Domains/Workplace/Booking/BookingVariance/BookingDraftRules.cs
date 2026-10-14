namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking draft must pass. Check returns its problems, or none.</summary>
public interface IBookingDraftRule
{
    List<string> Check(BookingDraft item);
}

public sealed record BookingDraftCodeLengthRule(int MaxLength) : IBookingDraftRule
{
    public List<string> Check(BookingDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingDraftTitleLengthRule(int MaxLength) : IBookingDraftRule
{
    public List<string> Check(BookingDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingDraftPhoneLengthRule(int MaxLength) : IBookingDraftRule
{
    public List<string> Check(BookingDraft item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingDraftRequiredPhoneRule(string Message) : IBookingDraftRule
{
    public List<string> Check(BookingDraft item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingDraftApprovedGuardRule(double Limit) : IBookingDraftRule
{
    public List<string> Check(BookingDraft item)
    {
        if (item.Approved && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record BookingDraftAllowedPhoneRule(List<string> Allowed) : IBookingDraftRule
{
    public List<string> Check(BookingDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking draft.</summary>
public sealed record BookingDraftValidator(List<IBookingDraftRule> Rules)
{
    public List<string> Validate(BookingDraft item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(BookingDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingDraft> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of booking draft rules.</summary>
public static class BookingDraftRules
{
    public static BookingDraftValidator DefaultBookingDraftValidator()
    {
        return new BookingDraftValidator(
            new List<IBookingDraftRule>
            {
                new BookingDraftCodeLengthRule(64),
                new BookingDraftTitleLengthRule(3600),
                new BookingDraftPhoneLengthRule(255),
                new BookingDraftRequiredPhoneRule("booking draft needs this field"),
                new BookingDraftApprovedGuardRule(3.4),
                new BookingDraftAllowedPhoneRule(new List<string> { "adaptive", "rolling", "scheduled" }),
            });
    }
}
