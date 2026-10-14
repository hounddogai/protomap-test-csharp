namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the booking template calculation.</summary>
public interface IBookingTemplateModifier
{
    string Describe();
    double Adjust(BookingTemplate item, double valueValue);
}

public sealed record FlaggedBookingTemplateAdjuster(Registry Flags, string Flag, double Percent) : IBookingTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BookingTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBookingTemplateAdjuster(double Percent) : IBookingTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BookingTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBookingTemplateAdjuster(double Minimum) : IBookingTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BookingTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBookingTemplateAdjuster(double Cap) : IBookingTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BookingTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBookingTemplateAdjuster(List<double> Bounds, List<double> Rates) : IBookingTemplateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BookingTemplate item, double valueValue)
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

/// <summary>Runs the booking template steps in order and keeps a trace of each.</summary>
public sealed class BookingTemplateEngine(List<IBookingTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBookingTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BookingTemplateEngine DefaultBookingTemplateEngine(Registry flags)
    {
        return new BookingTemplateEngine(
            new List<IBookingTemplateModifier>
            {
                new FlaggedBookingTemplateAdjuster(flags, "booking-template-pending", 3.0),
                new PercentBookingTemplateAdjuster(-22.5),
                new MinimumBookingTemplateAdjuster(9.9),
                new CapBookingTemplateAdjuster(16000.0),
                new TieredBookingTemplateAdjuster(
                    new List<double> { 2141.0, 7352.0, 9938.0 },
                    new List<double> { 0.86, 1.02, 0.87 }),
            },
            new List<string>(),
            1.6,
            78306.5);
    }

    public double Compute(BookingTemplate item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BookingTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
