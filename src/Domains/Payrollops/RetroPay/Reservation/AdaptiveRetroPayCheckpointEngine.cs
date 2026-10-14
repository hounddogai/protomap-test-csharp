namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive retro pay checkpoint calculation.</summary>
public interface IAdaptiveRetroPayCheckpointFactor
{
    string Describe();
    double Adjust(AdaptiveRetroPayCheckpoint item, double valueValue);
}

public sealed record AdaptiveRetroPayCheckpointThresholdOffsetAdjuster(double Weight) : IAdaptiveRetroPayCheckpointFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(AdaptiveRetroPayCheckpoint item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record FlaggedAdaptiveRetroPayCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveRetroPayCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveRetroPayCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveRetroPayCheckpointAdjuster(double Minimum) : IAdaptiveRetroPayCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveRetroPayCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdaptiveRetroPayCheckpointAdjuster(double Cap) : IAdaptiveRetroPayCheckpointFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveRetroPayCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameAdaptiveRetroPayCheckpointAdjuster(Dictionary<string, double> Factors) : IAdaptiveRetroPayCheckpointFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveRetroPayCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive retro pay checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveRetroPayCheckpointEngine(List<IAdaptiveRetroPayCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveRetroPayCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveRetroPayCheckpointEngine DefaultAdaptiveRetroPayCheckpointEngine(Registry flags)
    {
        return new AdaptiveRetroPayCheckpointEngine(
            new List<IAdaptiveRetroPayCheckpointFactor>
            {
                new AdaptiveRetroPayCheckpointThresholdOffsetAdjuster(0.88),
                new FlaggedAdaptiveRetroPayCheckpointAdjuster(flags, "adaptive-retro-pay-checkpoint-tiered", 6.9),
                new MinimumAdaptiveRetroPayCheckpointAdjuster(31.5),
                new CapAdaptiveRetroPayCheckpointAdjuster(4509.0),
                new NameAdaptiveRetroPayCheckpointAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.32, ["west"] = 1.37, ["east"] = 1.23 }),
            },
            new List<string>(),
            2.2,
            52363.1);
    }

    public double Compute(AdaptiveRetroPayCheckpoint item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveRetroPayCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
