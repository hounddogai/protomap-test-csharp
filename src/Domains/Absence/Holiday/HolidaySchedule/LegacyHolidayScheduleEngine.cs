namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy holiday schedule calculation.</summary>
public interface ILegacyHolidayScheduleAdjuster
{
    string Describe();
    double Adjust(LegacyHolidaySchedule item, double valueValue);
}

public sealed record PercentLegacyHolidayScheduleAdjuster(double Percent) : ILegacyHolidayScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyHolidaySchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LegacyHolidayScheduleUpdatedAtOffsetAdjuster(double Weight) : ILegacyHolidayScheduleAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(LegacyHolidaySchedule item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record FlaggedLegacyHolidayScheduleAdjuster(Registry Flags, string Flag, double Percent) : ILegacyHolidayScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyHolidaySchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyHolidayScheduleAdjuster(double Minimum) : ILegacyHolidayScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyHolidaySchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLegacyHolidayScheduleAdjuster(List<double> Bounds, List<double> Rates) : ILegacyHolidayScheduleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyHolidaySchedule item, double valueValue)
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

public sealed record CapLegacyHolidayScheduleAdjuster(double Cap) : ILegacyHolidayScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyHolidaySchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy holiday schedule steps in order and keeps a trace of each.</summary>
public sealed class LegacyHolidayScheduleEngine(List<ILegacyHolidayScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyHolidayScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyHolidayScheduleEngine DefaultLegacyHolidayScheduleEngine(Registry flags)
    {
        return new LegacyHolidayScheduleEngine(
            new List<ILegacyHolidayScheduleAdjuster>
            {
                new PercentLegacyHolidayScheduleAdjuster(9.6),
                new LegacyHolidayScheduleUpdatedAtOffsetAdjuster(1.44),
                new FlaggedLegacyHolidayScheduleAdjuster(flags, "legacy-holiday-schedule-scheduled", 8.0),
                new MinimumLegacyHolidayScheduleAdjuster(9.0),
                new TieredLegacyHolidayScheduleAdjuster(
                    new List<double> { 3590.0, 3846.0, 6778.0 },
                    new List<double> { 0.89, 1.05, 0.97 }),
                new CapLegacyHolidayScheduleAdjuster(4730.0),
            },
            new List<string>(),
            2.3,
            35383.9);
    }

    public double Price(LegacyHolidaySchedule item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyHolidaySchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
