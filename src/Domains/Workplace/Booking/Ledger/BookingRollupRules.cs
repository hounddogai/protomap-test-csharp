namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a booking rollup must pass. Check returns its problems, or none.</summary>
public interface IBookingRollupRule
{
    List<string> Check(BookingRollup item);
}

public sealed record BookingRollupAllowedTitleRule(List<string> Allowed) : IBookingRollupRule
{
    public List<string> Check(BookingRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

public sealed record BookingRollupFteRangeRule(double Min, double Max) : IBookingRollupRule
{
    public List<string> Check(BookingRollup item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingRollupSourceLengthRule(int MaxLength) : IBookingRollupRule
{
    public List<string> Check(BookingRollup item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingRollupRequiredTitleRule(string Message) : IBookingRollupRule
{
    public List<string> Check(BookingRollup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking rollup.</summary>
public sealed record BookingRollupValidator(List<IBookingRollupRule> Rules)
{
    public List<string> Validate(BookingRollup item)
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

    public void RequireValid(BookingRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingRollup> items)
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

/// <summary>Functions of booking rollup rules.</summary>
public static class BookingRollupRules
{
    public static BookingRollupValidator DefaultBookingRollupValidator()
    {
        return new BookingRollupValidator(
            new List<IBookingRollupRule>
            {
                new BookingRollupAllowedTitleRule(new List<string> { "legacy", "pending" }),
                new BookingRollupFteRangeRule(7.5, 3841.1),
                new BookingRollupSourceLengthRule(16),
                new BookingRollupRequiredTitleRule("booking rollup needs an entry"),
            });
    }
}
