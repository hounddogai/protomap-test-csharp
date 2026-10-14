namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled booking template calculation.</summary>
public interface IScheduledBookingTemplateModifier
{
    string Describe();
    double Adjust(ScheduledBookingTemplate item, double valueValue);
}

public sealed record FlaggedScheduledBookingTemplateAdjuster(Registry Flags, string Flag, double Percent) : IScheduledBookingTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledBookingTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentScheduledBookingTemplateAdjuster(double Percent) : IScheduledBookingTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledBookingTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledBookingTemplateAdjuster(List<double> Bounds, List<double> Rates) : IScheduledBookingTemplateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledBookingTemplate item, double valueValue)
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

public sealed record CapScheduledBookingTemplateAdjuster(double Cap) : IScheduledBookingTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledBookingTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledBookingTemplateAdjuster(double Minimum) : IScheduledBookingTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledBookingTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameScheduledBookingTemplateAdjuster(Dictionary<string, double> Factors) : IScheduledBookingTemplateModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScheduledBookingTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled booking template steps in order and keeps a trace of each.</summary>
public sealed class ScheduledBookingTemplateEngine(List<IScheduledBookingTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledBookingTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledBookingTemplateEngine DefaultScheduledBookingTemplateEngine(Registry flags)
    {
        return new ScheduledBookingTemplateEngine(
            new List<IScheduledBookingTemplateModifier>
            {
                new FlaggedScheduledBookingTemplateAdjuster(flags, "scheduled-booking-template-capped", 5.8),
                new PercentScheduledBookingTemplateAdjuster(23.5),
                new TieredScheduledBookingTemplateAdjuster(
                    new List<double> { 2612.0, 3717.0, 5337.0, 6018.0 },
                    new List<double> { 0.88, 0.91, 0.96, 0.81 }),
                new CapScheduledBookingTemplateAdjuster(2115.0),
                new MinimumScheduledBookingTemplateAdjuster(33.9),
                new NameScheduledBookingTemplateAdjuster(new Dictionary<string, double> { ["west"] = 1.01 }),
            },
            new List<string>(),
            4.3,
            89133.5);
    }

    public double Evaluate(ScheduledBookingTemplate item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledBookingTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
