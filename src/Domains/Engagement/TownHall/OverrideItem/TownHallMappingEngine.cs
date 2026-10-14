namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

/// <summary>One step of the town hall mapping calculation.</summary>
public interface ITownHallMappingStep
{
    string Describe();
    double Adjust(TownHallMapping item, double valueValue);
}

public sealed record TieredTownHallMappingAdjuster(List<double> Bounds, List<double> Rates) : ITownHallMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallMapping item, double valueValue)
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

public sealed record MinimumTownHallMappingAdjuster(double Minimum) : ITownHallMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTownHallMappingAdjuster(double Cap) : ITownHallMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TownHallMappingVersionOffsetAdjuster(double Weight) : ITownHallMappingStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(TownHallMapping item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the town hall mapping steps in order and keeps a trace of each.</summary>
public sealed class TownHallMappingEngine(List<ITownHallMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallMappingEngine DefaultTownHallMappingEngine()
    {
        return new TownHallMappingEngine(
            new List<ITownHallMappingStep>
            {
                new TieredTownHallMappingAdjuster(
                    new List<double> { 5122.0, 8591.0 },
                    new List<double> { 0.91, 1.07 }),
                new MinimumTownHallMappingAdjuster(10.6),
                new CapTownHallMappingAdjuster(6884.0),
                new TownHallMappingVersionOffsetAdjuster(1.4),
            },
            new List<string>(),
            2.2,
            23796.1);
    }

    public double Compute(TownHallMapping item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
