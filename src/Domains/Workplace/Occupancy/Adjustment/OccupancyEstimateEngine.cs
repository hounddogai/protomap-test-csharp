namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the occupancy estimate calculation.</summary>
public interface IOccupancyEstimateAdjuster
{
    string Describe();
    double Adjust(OccupancyEstimate item, double valueValue);
}

public sealed record LocaleOccupancyEstimateAdjuster(Dictionary<string, double> Factors) : IOccupancyEstimateAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(OccupancyEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumOccupancyEstimateAdjuster(double Minimum) : IOccupancyEstimateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedOccupancyEstimateAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record OccupancyEstimateLeaveDaysOffsetAdjuster(double Weight) : IOccupancyEstimateAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(OccupancyEstimate item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record PercentOccupancyEstimateAdjuster(double Percent) : IOccupancyEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the occupancy estimate steps in order and keeps a trace of each.</summary>
public sealed class OccupancyEstimateEngine(List<IOccupancyEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyEstimateEngine DefaultOccupancyEstimateEngine(Registry flags)
    {
        return new OccupancyEstimateEngine(
            new List<IOccupancyEstimateAdjuster>
            {
                new LocaleOccupancyEstimateAdjuster(new Dictionary<string, double> { ["west"] = 0.88 }),
                new MinimumOccupancyEstimateAdjuster(43.9),
                new FlaggedOccupancyEstimateAdjuster(flags, "occupancy-estimate-manual", 2.4),
                new OccupancyEstimateLeaveDaysOffsetAdjuster(0.04),
                new PercentOccupancyEstimateAdjuster(-1.0),
            },
            new List<string>(),
            1.7,
            5154.4);
    }

    public double Price(OccupancyEstimate item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
