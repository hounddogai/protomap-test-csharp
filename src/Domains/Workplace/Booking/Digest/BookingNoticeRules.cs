namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking notice must pass. Check returns its problems, or none.</summary>
public interface IBookingNoticeRule
{
    List<string> Check(BookingNotice item);
}

public sealed record BookingNoticeTitleLengthRule(int MaxLength) : IBookingNoticeRule
{
    public List<string> Check(BookingNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingNoticeAllowedTitleRule(List<string> Allowed) : IBookingNoticeRule
{
    public List<string> Check(BookingNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record BookingNoticeSegmentLengthRule(int MaxLength) : IBookingNoticeRule
{
    public List<string> Check(BookingNotice item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingNoticeCodePrefixRule(string Prefix) : IBookingNoticeRule
{
    public List<string> Check(BookingNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BookingNoticeRatioRangeRule(double Min, double Max) : IBookingNoticeRule
{
    public List<string> Check(BookingNotice item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a booking notice.</summary>
public sealed record BookingNoticeValidator(List<IBookingNoticeRule> Rules)
{
    public List<string> Validate(BookingNotice item)
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

    public void RequireValid(BookingNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingNotice> items)
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

/// <summary>Functions of booking notice rules.</summary>
public static class BookingNoticeRules
{
    public static BookingNoticeValidator DefaultBookingNoticeValidator()
    {
        return new BookingNoticeValidator(
            new List<IBookingNoticeRule>
            {
                new BookingNoticeTitleLengthRule(1000),
                new BookingNoticeAllowedTitleRule(new List<string> { "weighted" }),
                new BookingNoticeSegmentLengthRule(64),
                new BookingNoticeCodePrefixRule("OPS"),
                new BookingNoticeRatioRangeRule(9.0, 799.4),
            });
    }
}
