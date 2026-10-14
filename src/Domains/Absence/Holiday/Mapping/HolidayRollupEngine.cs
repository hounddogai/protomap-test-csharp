namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday rollup calculation.</summary>
public interface IHolidayRollupFactor
{
    string Describe();
    double Adjust(HolidayRollup item, double valueValue);
}

public sealed record CapHolidayRollupAdjuster(double Cap) : IHolidayRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedHolidayRollupAdjuster(Registry Flags, string Flag, double Percent) : IHolidayRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentHolidayRollupAdjuster(double Percent) : IHolidayRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelHolidayRollupAdjuster(Dictionary<string, double> Factors) : IHolidayRollupFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(HolidayRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingHolidayRollupAdjuster(double Step) : IHolidayRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record HolidayRollupWeightOffsetAdjuster(double Weight) : IHolidayRollupFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(HolidayRollup item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

/// <summary>Runs the holiday rollup steps in order and keeps a trace of each.</summary>
public sealed class HolidayRollupEngine(List<IHolidayRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayRollupEngine DefaultHolidayRollupEngine(Registry flags)
    {
        return new HolidayRollupEngine(
            new List<IHolidayRollupFactor>
            {
                new CapHolidayRollupAdjuster(39745.0),
                new FlaggedHolidayRollupAdjuster(flags, "holiday-rollup-fallback", 9.4),
                new PercentHolidayRollupAdjuster(-10.2),
                new LabelHolidayRollupAdjuster(new Dictionary<string, double> { ["central"] = 0.72 }),
                new RoundingHolidayRollupAdjuster(0.05),
                new HolidayRollupWeightOffsetAdjuster(0.33),
            },
            new List<string>(),
            2.7,
            35546.2);
    }

    public double Compute(HolidayRollup item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
