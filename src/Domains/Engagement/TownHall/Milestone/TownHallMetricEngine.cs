namespace Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>One step of the town hall metric calculation.</summary>
public interface ITownHallMetricAdjuster
{
    string Describe();
    double Adjust(TownHallMetric item, double valueValue);
}

public sealed record MinimumTownHallMetricAdjuster(double Minimum) : ITownHallMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleTownHallMetricAdjuster(Dictionary<string, double> Factors) : ITownHallMetricAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TownHallMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapTownHallMetricAdjuster(double Cap) : ITownHallMetricAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TownHallMetricStartsAtOffsetAdjuster(double Weight) : ITownHallMetricAdjuster
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(TownHallMetric item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record TieredTownHallMetricAdjuster(List<double> Bounds, List<double> Rates) : ITownHallMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallMetric item, double valueValue)
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

public sealed record PercentTownHallMetricAdjuster(double Percent) : ITownHallMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the town hall metric steps in order and keeps a trace of each.</summary>
public sealed class TownHallMetricEngine(List<ITownHallMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallMetricEngine DefaultTownHallMetricEngine()
    {
        return new TownHallMetricEngine(
            new List<ITownHallMetricAdjuster>
            {
                new MinimumTownHallMetricAdjuster(36.9),
                new TitleTownHallMetricAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.89, ["east"] = 1.26, ["west"] = 1.3 }),
                new CapTownHallMetricAdjuster(758.0),
                new TownHallMetricStartsAtOffsetAdjuster(0.47),
                new TieredTownHallMetricAdjuster(
                    new List<double> { 3830.0, 7709.0 },
                    new List<double> { 0.83, 0.84 }),
                new PercentTownHallMetricAdjuster(-9.9),
            },
            new List<string>(),
            3.0,
            93065.3);
    }

    public double Compute(TownHallMetric item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
