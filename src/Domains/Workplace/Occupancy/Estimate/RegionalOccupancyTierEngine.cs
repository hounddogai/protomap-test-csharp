namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional occupancy tier calculation.</summary>
public interface IRegionalOccupancyTierAdjuster
{
    string Describe();
    double Adjust(RegionalOccupancyTier item, double valueValue);
}

public sealed record RegionalOccupancyTierRateOffsetAdjuster(double Weight) : IRegionalOccupancyTierAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(RegionalOccupancyTier item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record MinimumRegionalOccupancyTierAdjuster(double Minimum) : IRegionalOccupancyTierAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalOccupancyTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRegionalOccupancyTierAdjuster(double Cap) : IRegionalOccupancyTierAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalOccupancyTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRegionalOccupancyTierAdjuster(double Step) : IRegionalOccupancyTierAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalOccupancyTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamRegionalOccupancyTierAdjuster(Dictionary<string, double> Factors) : IRegionalOccupancyTierAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(RegionalOccupancyTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRegionalOccupancyTierAdjuster(List<double> Bounds, List<double> Rates) : IRegionalOccupancyTierAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalOccupancyTier item, double valueValue)
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

/// <summary>Runs the regional occupancy tier steps in order and keeps a trace of each.</summary>
public sealed class RegionalOccupancyTierEngine(List<IRegionalOccupancyTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalOccupancyTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalOccupancyTierEngine DefaultRegionalOccupancyTierEngine()
    {
        return new RegionalOccupancyTierEngine(
            new List<IRegionalOccupancyTierAdjuster>
            {
                new RegionalOccupancyTierRateOffsetAdjuster(0.75),
                new MinimumRegionalOccupancyTierAdjuster(45.9),
                new CapRegionalOccupancyTierAdjuster(49821.0),
                new RoundingRegionalOccupancyTierAdjuster(5.0),
                new TeamRegionalOccupancyTierAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.73, ["retail"] = 0.92 }),
                new TieredRegionalOccupancyTierAdjuster(
                    new List<double> { 8774.0, 9330.0 },
                    new List<double> { 1.0, 1.15 }),
            },
            new List<string>(),
            4.0,
            45038.0);
    }

    public double Compute(RegionalOccupancyTier item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalOccupancyTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
