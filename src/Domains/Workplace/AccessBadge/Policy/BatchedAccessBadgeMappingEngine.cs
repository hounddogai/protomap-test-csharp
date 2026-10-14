namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the batched access badge mapping calculation.</summary>
public interface IBatchedAccessBadgeMappingAdjuster
{
    string Describe();
    double Adjust(BatchedAccessBadgeMapping item, double valueValue);
}

public sealed record TieredBatchedAccessBadgeMappingAdjuster(List<double> Bounds, List<double> Rates) : IBatchedAccessBadgeMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedAccessBadgeMapping item, double valueValue)
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

public sealed record BatchedAccessBadgeMappingLeaveDaysOffsetAdjuster(double Weight) : IBatchedAccessBadgeMappingAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(BatchedAccessBadgeMapping item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record FlaggedBatchedAccessBadgeMappingAdjuster(Registry Flags, string Flag, double Percent) : IBatchedAccessBadgeMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedAccessBadgeMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryBatchedAccessBadgeMappingAdjuster(Dictionary<string, double> Factors) : IBatchedAccessBadgeMappingAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(BatchedAccessBadgeMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched access badge mapping steps in order and keeps a trace of each.</summary>
public sealed class BatchedAccessBadgeMappingEngine(List<IBatchedAccessBadgeMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedAccessBadgeMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedAccessBadgeMappingEngine DefaultBatchedAccessBadgeMappingEngine(Registry flags)
    {
        return new BatchedAccessBadgeMappingEngine(
            new List<IBatchedAccessBadgeMappingAdjuster>
            {
                new TieredBatchedAccessBadgeMappingAdjuster(
                    new List<double> { 5687.0, 7273.0, 7746.0 },
                    new List<double> { 1.12, 0.84, 0.97 }),
                new BatchedAccessBadgeMappingLeaveDaysOffsetAdjuster(1.79),
                new FlaggedBatchedAccessBadgeMappingAdjuster(flags, "batched-access-badge-mapping-seasonal", 4.7),
                new CategoryBatchedAccessBadgeMappingAdjuster(new Dictionary<string, double> { ["retail"] = 1.06 }),
            },
            new List<string>(),
            4.1,
            2784.2);
    }

    public double Compute(BatchedAccessBadgeMapping item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedAccessBadgeMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
