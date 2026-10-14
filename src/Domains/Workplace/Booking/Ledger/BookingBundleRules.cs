namespace Acme.Workforce.Workplace.Booking.Ledger;

/// <summary>A rule a booking bundle must pass. Check returns its problems, or none.</summary>
public interface IBookingBundleRule
{
    List<string> Check(BookingBundle item);
}

public sealed record BookingBundleRatioRangeRule(double Min, double Max) : IBookingBundleRule
{
    public List<string> Check(BookingBundle item)
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

public sealed record BookingBundleDiscountRangeRule(double Min, double Max) : IBookingBundleRule
{
    public List<string> Check(BookingBundle item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingBundleLabelLengthRule(int MaxLength) : IBookingBundleRule
{
    public List<string> Check(BookingBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking bundle.</summary>
public sealed record BookingBundleValidator(List<IBookingBundleRule> Rules)
{
    public List<string> Validate(BookingBundle item)
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

    public void RequireValid(BookingBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingBundle> items)
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

/// <summary>Functions of booking bundle rules.</summary>
public static class BookingBundleRules
{
    public static BookingBundleValidator DefaultBookingBundleValidator()
    {
        return new BookingBundleValidator(
            new List<IBookingBundleRule>
            {
                new BookingBundleRatioRangeRule(4.2, 1740.8),
                new BookingBundleDiscountRangeRule(9.0, 397.9),
                new BookingBundleLabelLengthRule(86400),
            });
    }
}
