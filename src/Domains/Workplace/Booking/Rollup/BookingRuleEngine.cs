namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the booking rule calculation.</summary>
public interface IBookingRuleFactor
{
    string Describe();
    double Adjust(BookingRule item, double valueValue);
}

public sealed record BookingRuleRateOffsetAdjuster(double Weight) : IBookingRuleFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(BookingRule item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record RoundingBookingRuleAdjuster(double Step) : IBookingRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BookingRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumBookingRuleAdjuster(double Minimum) : IBookingRuleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBookingRuleAdjuster(double Percent) : IBookingRuleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBookingRuleAdjuster(List<double> Bounds, List<double> Rates) : IBookingRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingRule item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record CapBookingRuleAdjuster(double Cap) : IBookingRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the booking rule steps in order and keeps a trace of each.</summary>
public sealed class BookingRuleEngine(List<IBookingRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingRuleEngine DefaultBookingRuleEngine()
    {
        return new BookingRuleEngine(
            new List<IBookingRuleFactor>
            {
                new BookingRuleRateOffsetAdjuster(1.89),
                new RoundingBookingRuleAdjuster(5.0),
                new MinimumBookingRuleAdjuster(20.7),
                new PercentBookingRuleAdjuster(-17.4),
                new TieredBookingRuleAdjuster(new List<double> { 5220.0, 6082.0 }, new List<double> { 1.06, 1.05 }),
                new CapBookingRuleAdjuster(15357.0),
            },
            new List<string>(),
            0.6,
            28455.8);
    }

    public double Price(BookingRule item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<BookingRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
