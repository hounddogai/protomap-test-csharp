namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the merit cycle batch calculation.</summary>
public interface IMeritCycleBatchAdjuster
{
    string Describe();
    double Adjust(MeritCycleBatch item, double valueValue);
}

public sealed record PercentMeritCycleBatchAdjuster(double Percent) : IMeritCycleBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionMeritCycleBatchAdjuster(Dictionary<string, double> Factors) : IMeritCycleBatchAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(MeritCycleBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredMeritCycleBatchAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleBatchAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleBatch item, double valueValue)
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

public sealed record MeritCycleBatchCreatedAtOffsetAdjuster(double Weight) : IMeritCycleBatchAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(MeritCycleBatch item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record FlaggedMeritCycleBatchAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the merit cycle batch steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleBatchEngine(List<IMeritCycleBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleBatchEngine DefaultMeritCycleBatchEngine(Registry flags)
    {
        return new MeritCycleBatchEngine(
            new List<IMeritCycleBatchAdjuster>
            {
                new PercentMeritCycleBatchAdjuster(19.0),
                new RegionMeritCycleBatchAdjuster(new Dictionary<string, double> { ["central"] = 1.04 }),
                new TieredMeritCycleBatchAdjuster(
                    new List<double> { 2325.0, 3682.0, 5670.0 },
                    new List<double> { 1.2, 1.19, 0.96 }),
                new MeritCycleBatchCreatedAtOffsetAdjuster(0.54),
                new FlaggedMeritCycleBatchAdjuster(flags, "merit-cycle-batch-strict", -3.4),
            },
            new List<string>(),
            1.8,
            99423.1);
    }

    public double Price(MeritCycleBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
