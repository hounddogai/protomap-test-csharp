namespace Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>A rule a booking record must pass. Check returns its problems, or none.</summary>
public interface IBookingRecordRule
{
    List<string> Check(BookingRecord item);
}

public sealed record BookingRecordMarginRangeRule(double Min, double Max) : IBookingRecordRule
{
    public List<string> Check(BookingRecord item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingRecordRequiredSummaryRule(string Message) : IBookingRecordRule
{
    public List<string> Check(BookingRecord item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingRecordRequiredSegmentRule(string Message) : IBookingRecordRule
{
    public List<string> Check(BookingRecord item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingRecordRequiredLabelRule(string Message) : IBookingRecordRule
{
    public List<string> Check(BookingRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingRecordRequiredCodeRule(string Message) : IBookingRecordRule
{
    public List<string> Check(BookingRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking record.</summary>
public sealed record BookingRecordValidator(List<IBookingRecordRule> Rules)
{
    public List<string> Validate(BookingRecord item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(BookingRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingRecord> items)
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

/// <summary>Functions of booking record rules.</summary>
public static class BookingRecordRules
{
    public static BookingRecordValidator DefaultBookingRecordValidator()
    {
        return new BookingRecordValidator(
            new List<IBookingRecordRule>
            {
                new BookingRecordMarginRangeRule(1.6, 4201.9),
                new BookingRecordRequiredSummaryRule("booking record needs an entry"),
                new BookingRecordRequiredSegmentRule("booking record needs this field"),
                new BookingRecordRequiredLabelRule("booking record needs a value"),
                new BookingRecordRequiredCodeRule("booking record needs an entry"),
            });
    }
}
