namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global position rollup calculation.</summary>
public interface IGlobalPositionRollupStep
{
    string Describe();
    double Adjust(GlobalPositionRollup item, double valueValue);
}

public sealed record TieredGlobalPositionRollupAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPositionRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPositionRollup item, double valueValue)
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

public sealed record PercentGlobalPositionRollupAdjuster(double Percent) : IGlobalPositionRollupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalPositionRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitGlobalPositionRollupAdjuster(Dictionary<string, double> Factors) : IGlobalPositionRollupStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(GlobalPositionRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record GlobalPositionRollupRatioOffsetAdjuster(double Weight) : IGlobalPositionRollupStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(GlobalPositionRollup item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record MinimumGlobalPositionRollupAdjuster(double Minimum) : IGlobalPositionRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalPositionRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global position rollup steps in order and keeps a trace of each.</summary>
public sealed class GlobalPositionRollupEngine(List<IGlobalPositionRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPositionRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPositionRollupEngine DefaultGlobalPositionRollupEngine()
    {
        return new GlobalPositionRollupEngine(
            new List<IGlobalPositionRollupStep>
            {
                new TieredGlobalPositionRollupAdjuster(
                    new List<double> { 1916.0, 6349.0 },
                    new List<double> { 1.11, 0.89 }),
                new PercentGlobalPositionRollupAdjuster(3.2),
                new UnitGlobalPositionRollupAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.23, ["south"] = 0.76, ["retail"] = 0.88 }),
                new GlobalPositionRollupRatioOffsetAdjuster(0.04),
                new MinimumGlobalPositionRollupAdjuster(20.9),
            },
            new List<string>(),
            4.9,
            82081.8);
    }

    public double Evaluate(GlobalPositionRollup item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPositionRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
