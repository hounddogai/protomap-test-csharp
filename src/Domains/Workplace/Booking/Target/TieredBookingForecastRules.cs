namespace Acme.Workforce.Workplace.Booking.Target;

/// <summary>A rule a tiered booking forecast must pass. Check returns its problems, or none.</summary>
public interface ITieredBookingForecastRule
{
    List<string> Check(TieredBookingForecast item);
}

public sealed record TieredBookingForecastLabelLengthRule(int MaxLength) : ITieredBookingForecastRule
{
    public List<string> Check(TieredBookingForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBookingForecastRequiredNameRule(string Message) : ITieredBookingForecastRule
{
    public List<string> Check(TieredBookingForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBookingForecastCodePrefixRule(string Prefix) : ITieredBookingForecastRule
{
    public List<string> Check(TieredBookingForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBookingForecastHoursRangeRule(double Min, double Max) : ITieredBookingForecastRule
{
    public List<string> Check(TieredBookingForecast item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredBookingForecastNameLengthRule(int MaxLength) : ITieredBookingForecastRule
{
    public List<string> Check(TieredBookingForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBookingForecastRequiredCodeRule(string Message) : ITieredBookingForecastRule
{
    public List<string> Check(TieredBookingForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered booking forecast.</summary>
public sealed record TieredBookingForecastValidator(List<ITieredBookingForecastRule> Rules)
{
    public List<string> Validate(TieredBookingForecast item)
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

    public void RequireValid(TieredBookingForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredBookingForecast> items)
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

/// <summary>Functions of tiered booking forecast rules.</summary>
public static class TieredBookingForecastRules
{
    public static TieredBookingForecastValidator DefaultTieredBookingForecastValidator()
    {
        return new TieredBookingForecastValidator(
            new List<ITieredBookingForecastRule>
            {
                new TieredBookingForecastLabelLengthRule(255),
                new TieredBookingForecastRequiredNameRule("tiered booking forecast needs this field"),
                new TieredBookingForecastCodePrefixRule("REF"),
                new TieredBookingForecastHoursRangeRule(6.9, 1083.0),
                new TieredBookingForecastNameLengthRule(32),
                new TieredBookingForecastRequiredCodeRule("tiered booking forecast needs this field"),
            });
    }
}
