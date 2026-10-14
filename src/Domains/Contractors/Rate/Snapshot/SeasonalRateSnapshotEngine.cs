namespace Acme.Workforce.Contractors.Rate.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal rate snapshot calculation.</summary>
public interface ISeasonalRateSnapshotModifier
{
    string Describe();
    double Adjust(SeasonalRateSnapshot item, double valueValue);
}

public sealed record ChannelSeasonalRateSnapshotAdjuster(Dictionary<string, double> Factors) : ISeasonalRateSnapshotModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(SeasonalRateSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record SeasonalRateSnapshotHoursOffsetAdjuster(double Weight) : ISeasonalRateSnapshotModifier
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(SeasonalRateSnapshot item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record RoundingSeasonalRateSnapshotAdjuster(double Step) : ISeasonalRateSnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalRateSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSeasonalRateSnapshotAdjuster(double Minimum) : ISeasonalRateSnapshotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalRateSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSeasonalRateSnapshotAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalRateSnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalRateSnapshot item, double valueValue)
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

/// <summary>Runs the seasonal rate snapshot steps in order and keeps a trace of each.</summary>
public sealed class SeasonalRateSnapshotEngine(List<ISeasonalRateSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalRateSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalRateSnapshotEngine DefaultSeasonalRateSnapshotEngine()
    {
        return new SeasonalRateSnapshotEngine(
            new List<ISeasonalRateSnapshotModifier>
            {
                new ChannelSeasonalRateSnapshotAdjuster(new Dictionary<string, double> { ["north"] = 1.22 }),
                new SeasonalRateSnapshotHoursOffsetAdjuster(0.63),
                new RoundingSeasonalRateSnapshotAdjuster(0.05),
                new MinimumSeasonalRateSnapshotAdjuster(38.2),
                new TieredSeasonalRateSnapshotAdjuster(
                    new List<double> { 4324.0, 6341.0, 8398.0, 8758.0 },
                    new List<double> { 1.11, 1.12, 1.1, 0.98 }),
            },
            new List<string>(),
            0.8,
            89721.3);
    }

    public double Compute(SeasonalRateSnapshot item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalRateSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
