namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>One step of the weighted access badge batch calculation.</summary>
public interface IWeightedAccessBadgeBatchAdjuster
{
    string Describe();
    double Adjust(WeightedAccessBadgeBatch item, double valueValue);
}

public sealed record TieredWeightedAccessBadgeBatchAdjuster(List<double> Bounds, List<double> Rates) : IWeightedAccessBadgeBatchAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedAccessBadgeBatch item, double valueValue)
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

public sealed record PercentWeightedAccessBadgeBatchAdjuster(double Percent) : IWeightedAccessBadgeBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAccessBadgeBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedAccessBadgeBatchAdjuster(double Cap) : IWeightedAccessBadgeBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedAccessBadgeBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record WeightedAccessBadgeBatchEndsAtOffsetAdjuster(double Weight) : IWeightedAccessBadgeBatchAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(WeightedAccessBadgeBatch item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record LabelWeightedAccessBadgeBatchAdjuster(Dictionary<string, double> Factors) : IWeightedAccessBadgeBatchAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WeightedAccessBadgeBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the weighted access badge batch steps in order and keeps a trace of each.</summary>
public sealed class WeightedAccessBadgeBatchEngine(List<IWeightedAccessBadgeBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAccessBadgeBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAccessBadgeBatchEngine DefaultWeightedAccessBadgeBatchEngine()
    {
        return new WeightedAccessBadgeBatchEngine(
            new List<IWeightedAccessBadgeBatchAdjuster>
            {
                new TieredWeightedAccessBadgeBatchAdjuster(
                    new List<double> { 176.0, 464.0 },
                    new List<double> { 1.13, 0.86 }),
                new PercentWeightedAccessBadgeBatchAdjuster(21.5),
                new CapWeightedAccessBadgeBatchAdjuster(23230.0),
                new WeightedAccessBadgeBatchEndsAtOffsetAdjuster(1.24),
                new LabelWeightedAccessBadgeBatchAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.31, ["south"] = 0.76 }),
            },
            new List<string>(),
            3.0,
            86048.5);
    }

    public double Run(WeightedAccessBadgeBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAccessBadgeBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
