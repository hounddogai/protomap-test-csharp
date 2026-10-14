namespace Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>One step of the rolling refresh rollup calculation.</summary>
public interface IRollingRefreshRollupAdjuster
{
    string Describe();
    double Adjust(RollingRefreshRollup item, double valueValue);
}

public sealed record TieredRollingRefreshRollupAdjuster(List<double> Bounds, List<double> Rates) : IRollingRefreshRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingRefreshRollup item, double valueValue)
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

public sealed record UnitRollingRefreshRollupAdjuster(Dictionary<string, double> Factors) : IRollingRefreshRollupAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(RollingRefreshRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RollingRefreshRollupRatioOffsetAdjuster(double Weight) : IRollingRefreshRollupAdjuster
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(RollingRefreshRollup item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the rolling refresh rollup steps in order and keeps a trace of each.</summary>
public sealed class RollingRefreshRollupEngine(List<IRollingRefreshRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingRefreshRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingRefreshRollupEngine DefaultRollingRefreshRollupEngine()
    {
        return new RollingRefreshRollupEngine(
            new List<IRollingRefreshRollupAdjuster>
            {
                new TieredRollingRefreshRollupAdjuster(
                    new List<double> { 2611.0, 5398.0, 9973.0 },
                    new List<double> { 0.88, 1.14, 0.87 }),
                new UnitRollingRefreshRollupAdjuster(new Dictionary<string, double> { ["north"] = 1.14 }),
                new RollingRefreshRollupRatioOffsetAdjuster(0.7),
            },
            new List<string>(),
            3.3,
            85527.0);
    }

    public double Compute(RollingRefreshRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingRefreshRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
