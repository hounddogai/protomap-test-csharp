namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the occupancy cycle calculation.</summary>
public interface IOccupancyCycleStep
{
    string Describe();
    double Adjust(OccupancyCycle item, double valueValue);
}

public sealed record FlaggedOccupancyCycleAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyCycleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredOccupancyCycleAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyCycleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyCycle item, double valueValue)
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

public sealed record PercentOccupancyCycleAdjuster(double Percent) : IOccupancyCycleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameOccupancyCycleAdjuster(Dictionary<string, double> Factors) : IOccupancyCycleStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(OccupancyCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the occupancy cycle steps in order and keeps a trace of each.</summary>
public sealed class OccupancyCycleEngine(List<IOccupancyCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyCycleEngine DefaultOccupancyCycleEngine(Registry flags)
    {
        return new OccupancyCycleEngine(
            new List<IOccupancyCycleStep>
            {
                new FlaggedOccupancyCycleAdjuster(flags, "occupancy-cycle-capped", -2.1),
                new TieredOccupancyCycleAdjuster(
                    new List<double> { 2982.0, 3461.0, 7373.0 },
                    new List<double> { 1.0, 1.05, 1.03 }),
                new PercentOccupancyCycleAdjuster(0.4),
                new NameOccupancyCycleAdjuster(new Dictionary<string, double> { ["online"] = 0.94 }),
            },
            new List<string>(),
            4.2,
            65923.0);
    }

    public double Price(OccupancyCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
