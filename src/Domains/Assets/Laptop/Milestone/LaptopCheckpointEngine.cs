namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop checkpoint calculation.</summary>
public interface ILaptopCheckpointAdjuster
{
    string Describe();
    double Adjust(LaptopCheckpoint item, double valueValue);
}

public sealed record TieredLaptopCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ILaptopCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopCheckpoint item, double valueValue)
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

public sealed record RoundingLaptopCheckpointAdjuster(double Step) : ILaptopCheckpointAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLaptopCheckpointAdjuster(double Minimum) : ILaptopCheckpointAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLaptopCheckpointAdjuster(double Percent) : ILaptopCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLaptopCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ILaptopCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLaptopCheckpointAdjuster(double Cap) : ILaptopCheckpointAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the laptop checkpoint steps in order and keeps a trace of each.</summary>
public sealed class LaptopCheckpointEngine(List<ILaptopCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopCheckpointEngine DefaultLaptopCheckpointEngine(Registry flags)
    {
        return new LaptopCheckpointEngine(
            new List<ILaptopCheckpointAdjuster>
            {
                new TieredLaptopCheckpointAdjuster(
                    new List<double> { 3820.0, 4673.0, 7572.0 },
                    new List<double> { 1.06, 1.13, 0.96 }),
                new RoundingLaptopCheckpointAdjuster(1.0),
                new MinimumLaptopCheckpointAdjuster(11.8),
                new PercentLaptopCheckpointAdjuster(11.8),
                new FlaggedLaptopCheckpointAdjuster(flags, "laptop-checkpoint-weighted", -1.2),
                new CapLaptopCheckpointAdjuster(44200.0),
            },
            new List<string>(),
            0.7,
            9419.1);
    }

    public double Price(LaptopCheckpoint item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
