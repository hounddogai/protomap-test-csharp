namespace Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>One step of the town hall manifest calculation.</summary>
public interface ITownHallManifestFactor
{
    string Describe();
    double Adjust(TownHallManifest item, double valueValue);
}

public sealed record NameTownHallManifestAdjuster(Dictionary<string, double> Factors) : ITownHallManifestFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TownHallManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TownHallManifestThresholdOffsetAdjuster(double Weight) : ITownHallManifestFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(TownHallManifest item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record CapTownHallManifestAdjuster(double Cap) : ITownHallManifestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTownHallManifestAdjuster(double Percent) : ITownHallManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTownHallManifestAdjuster(List<double> Bounds, List<double> Rates) : ITownHallManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallManifest item, double valueValue)
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

/// <summary>Runs the town hall manifest steps in order and keeps a trace of each.</summary>
public sealed class TownHallManifestEngine(List<ITownHallManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallManifestEngine DefaultTownHallManifestEngine()
    {
        return new TownHallManifestEngine(
            new List<ITownHallManifestFactor>
            {
                new NameTownHallManifestAdjuster(new Dictionary<string, double> { ["south"] = 0.94 }),
                new TownHallManifestThresholdOffsetAdjuster(0.52),
                new CapTownHallManifestAdjuster(32990.0),
                new PercentTownHallManifestAdjuster(-20.1),
                new TieredTownHallManifestAdjuster(
                    new List<double> { 6737.0, 8062.0, 8888.0 },
                    new List<double> { 1.11, 0.84, 1.11 }),
            },
            new List<string>(),
            2.7,
            79645.5);
    }

    public double Compute(TownHallManifest item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
