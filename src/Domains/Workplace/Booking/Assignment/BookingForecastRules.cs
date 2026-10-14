namespace Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>A rule a booking forecast must pass. Check returns its problems, or none.</summary>
public interface IBookingForecastRule
{
    List<string> Check(BookingForecast item);
}

public sealed record BookingForecastCodePrefixRule(string Prefix) : IBookingForecastRule
{
    public List<string> Check(BookingForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BookingForecastCostCenterLengthRule(int MaxLength) : IBookingForecastRule
{
    public List<string> Check(BookingForecast item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BookingForecastRequiredTitleRule(string Message) : IBookingForecastRule
{
    public List<string> Check(BookingForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingForecastScoreRangeRule(double Min, double Max) : IBookingForecastRule
{
    public List<string> Check(BookingForecast item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BookingForecastCodeLengthRule(int MaxLength) : IBookingForecastRule
{
    public List<string> Check(BookingForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking forecast.</summary>
public sealed record BookingForecastValidator(List<IBookingForecastRule> Rules)
{
    public List<string> Validate(BookingForecast item)
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

    public void RequireValid(BookingForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingForecast> items)
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

/// <summary>Functions of booking forecast rules.</summary>
public static class BookingForecastRules
{
    public static BookingForecastValidator DefaultBookingForecastValidator()
    {
        return new BookingForecastValidator(
            new List<IBookingForecastRule>
            {
                new BookingForecastCodePrefixRule("X"),
                new BookingForecastCostCenterLengthRule(16),
                new BookingForecastRequiredTitleRule("booking forecast needs an entry"),
                new BookingForecastScoreRangeRule(9.8, 3450.8),
                new BookingForecastCodeLengthRule(80),
            });
    }
}
