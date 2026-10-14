namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted mobility rollup calculation.</summary>
public interface IWeightedMobilityRollupStep
{
    string Describe();
    double Adjust(WeightedMobilityRollup item, double valueValue);
}

public sealed record CapWeightedMobilityRollupAdjuster(double Cap) : IWeightedMobilityRollupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedMobilityRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameWeightedMobilityRollupAdjuster(Dictionary<string, double> Factors) : IWeightedMobilityRollupStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(WeightedMobilityRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWeightedMobilityRollupAdjuster(double Minimum) : IWeightedMobilityRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedMobilityRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingWeightedMobilityRollupAdjuster(double Step) : IWeightedMobilityRollupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedMobilityRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted mobility rollup steps in order and keeps a trace of each.</summary>
public sealed class WeightedMobilityRollupEngine(List<IWeightedMobilityRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedMobilityRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedMobilityRollupEngine DefaultWeightedMobilityRollupEngine()
    {
        return new WeightedMobilityRollupEngine(
            new List<IWeightedMobilityRollupStep>
            {
                new CapWeightedMobilityRollupAdjuster(33908.0),
                new NameWeightedMobilityRollupAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.95, ["south"] = 1.34, ["retail"] = 0.73 }),
                new MinimumWeightedMobilityRollupAdjuster(0.6),
                new RoundingWeightedMobilityRollupAdjuster(0.05),
            },
            new List<string>(),
            2.1,
            2316.1);
    }

    public double Price(WeightedMobilityRollup item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedMobilityRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
