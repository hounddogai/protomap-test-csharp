namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal booking template calculation.</summary>
public interface ISeasonalBookingTemplateFactor
{
    string Describe();
    double Adjust(SeasonalBookingTemplate item, double valueValue);
}

public sealed record SegmentSeasonalBookingTemplateAdjuster(Dictionary<string, double> Factors) : ISeasonalBookingTemplateFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(SeasonalBookingTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSeasonalBookingTemplateAdjuster(double Step) : ISeasonalBookingTemplateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalBookingTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSeasonalBookingTemplateAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalBookingTemplateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalBookingTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalBookingTemplateAdjuster(double Cap) : ISeasonalBookingTemplateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalBookingTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal booking template steps in order and keeps a trace of each.</summary>
public sealed class SeasonalBookingTemplateEngine(List<ISeasonalBookingTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalBookingTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalBookingTemplateEngine DefaultSeasonalBookingTemplateEngine(Registry flags)
    {
        return new SeasonalBookingTemplateEngine(
            new List<ISeasonalBookingTemplateFactor>
            {
                new SegmentSeasonalBookingTemplateAdjuster(new Dictionary<string, double> { ["north"] = 0.9 }),
                new RoundingSeasonalBookingTemplateAdjuster(0.05),
                new FlaggedSeasonalBookingTemplateAdjuster(flags, "seasonal-booking-template-tiered", -7.1),
                new CapSeasonalBookingTemplateAdjuster(29996.0),
            },
            new List<string>(),
            0.7,
            46115.0);
    }

    public double Price(SeasonalBookingTemplate item)
    {
        var current = item.ProjectedValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<SeasonalBookingTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
