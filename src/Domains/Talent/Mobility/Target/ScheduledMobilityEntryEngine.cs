namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled mobility entry calculation.</summary>
public interface IScheduledMobilityEntryAdjuster
{
    string Describe();
    double Adjust(ScheduledMobilityEntry item, double valueValue);
}

public sealed record PercentScheduledMobilityEntryAdjuster(double Percent) : IScheduledMobilityEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledMobilityEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledMobilityEntryAdjuster(List<double> Bounds, List<double> Rates) : IScheduledMobilityEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledMobilityEntry item, double valueValue)
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

public sealed record CapScheduledMobilityEntryAdjuster(double Cap) : IScheduledMobilityEntryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledMobilityEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record UnitScheduledMobilityEntryAdjuster(Dictionary<string, double> Factors) : IScheduledMobilityEntryAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(ScheduledMobilityEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ScheduledMobilityEntryMaximumOffsetAdjuster(double Weight) : IScheduledMobilityEntryAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(ScheduledMobilityEntry item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record MinimumScheduledMobilityEntryAdjuster(double Minimum) : IScheduledMobilityEntryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledMobilityEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled mobility entry steps in order and keeps a trace of each.</summary>
public sealed class ScheduledMobilityEntryEngine(List<IScheduledMobilityEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledMobilityEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledMobilityEntryEngine DefaultScheduledMobilityEntryEngine()
    {
        return new ScheduledMobilityEntryEngine(
            new List<IScheduledMobilityEntryAdjuster>
            {
                new PercentScheduledMobilityEntryAdjuster(-9.9),
                new TieredScheduledMobilityEntryAdjuster(
                    new List<double> { 4713.0, 7138.0 },
                    new List<double> { 1.03, 0.89 }),
                new CapScheduledMobilityEntryAdjuster(17789.0),
                new UnitScheduledMobilityEntryAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.26, ["retail"] = 0.83, ["online"] = 0.83 }),
                new ScheduledMobilityEntryMaximumOffsetAdjuster(1.75),
                new MinimumScheduledMobilityEntryAdjuster(3.3),
            },
            new List<string>(),
            4.0,
            48090.8);
    }

    public double Price(ScheduledMobilityEntry item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledMobilityEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
