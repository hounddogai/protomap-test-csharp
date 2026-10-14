namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback holiday schedule calculation.</summary>
public interface IFallbackHolidayScheduleFactor
{
    string Describe();
    double Adjust(FallbackHolidaySchedule item, double valueValue);
}

public sealed record FlaggedFallbackHolidayScheduleAdjuster(Registry Flags, string Flag, double Percent) : IFallbackHolidayScheduleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackHolidaySchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFallbackHolidayScheduleAdjuster(double Percent) : IFallbackHolidayScheduleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackHolidaySchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackHolidayScheduleAdjuster(double Step) : IFallbackHolidayScheduleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackHolidaySchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapFallbackHolidayScheduleAdjuster(double Cap) : IFallbackHolidayScheduleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackHolidaySchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackHolidayScheduleAdjuster(List<double> Bounds, List<double> Rates) : IFallbackHolidayScheduleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackHolidaySchedule item, double valueValue)
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

/// <summary>Runs the fallback holiday schedule steps in order and keeps a trace of each.</summary>
public sealed class FallbackHolidayScheduleEngine(List<IFallbackHolidayScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackHolidayScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackHolidayScheduleEngine DefaultFallbackHolidayScheduleEngine(Registry flags)
    {
        return new FallbackHolidayScheduleEngine(
            new List<IFallbackHolidayScheduleFactor>
            {
                new FlaggedFallbackHolidayScheduleAdjuster(flags, "fallback-holiday-schedule-primary", -2.5),
                new PercentFallbackHolidayScheduleAdjuster(1.0),
                new RoundingFallbackHolidayScheduleAdjuster(0.5),
                new CapFallbackHolidayScheduleAdjuster(1799.0),
                new TieredFallbackHolidayScheduleAdjuster(
                    new List<double> { 3237.0, 7175.0, 8024.0 },
                    new List<double> { 0.97, 1.04, 0.83 }),
            },
            new List<string>(),
            0.1,
            9379.4);
    }

    public double Price(FallbackHolidaySchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackHolidaySchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
