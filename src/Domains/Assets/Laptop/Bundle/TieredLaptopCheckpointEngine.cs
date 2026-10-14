namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered laptop checkpoint calculation.</summary>
public interface ITieredLaptopCheckpointFactor
{
    string Describe();
    double Adjust(TieredLaptopCheckpoint item, double valueValue);
}

public sealed record LabelTieredLaptopCheckpointAdjuster(Dictionary<string, double> Factors) : ITieredLaptopCheckpointFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TieredLaptopCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTieredLaptopCheckpointAdjuster(double Percent) : ITieredLaptopCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredLaptopCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTieredLaptopCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ITieredLaptopCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredLaptopCheckpoint item, double valueValue)
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

public sealed record MinimumTieredLaptopCheckpointAdjuster(double Minimum) : ITieredLaptopCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredLaptopCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingTieredLaptopCheckpointAdjuster(double Step) : ITieredLaptopCheckpointFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredLaptopCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTieredLaptopCheckpointAdjuster(double Cap) : ITieredLaptopCheckpointFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredLaptopCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tiered laptop checkpoint steps in order and keeps a trace of each.</summary>
public sealed class TieredLaptopCheckpointEngine(List<ITieredLaptopCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredLaptopCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredLaptopCheckpointEngine DefaultTieredLaptopCheckpointEngine()
    {
        return new TieredLaptopCheckpointEngine(
            new List<ITieredLaptopCheckpointFactor>
            {
                new LabelTieredLaptopCheckpointAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.24, ["south"] = 1.0, ["east"] = 1.26 }),
                new PercentTieredLaptopCheckpointAdjuster(-19.2),
                new TieredTieredLaptopCheckpointAdjuster(
                    new List<double> { 4311.0, 4667.0, 5712.0, 7736.0 },
                    new List<double> { 0.87, 0.81, 1.16, 1.13 }),
                new MinimumTieredLaptopCheckpointAdjuster(30.1),
                new RoundingTieredLaptopCheckpointAdjuster(0.05),
                new CapTieredLaptopCheckpointAdjuster(40042.0),
            },
            new List<string>(),
            3.2,
            93409.0);
    }

    public double Run(TieredLaptopCheckpoint item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredLaptopCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
