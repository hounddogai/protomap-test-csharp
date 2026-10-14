namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay band checkpoint calculation.</summary>
public interface IPayBandCheckpointAdjuster
{
    string Describe();
    double Adjust(PayBandCheckpoint item, double valueValue);
}

public sealed record SegmentPayBandCheckpointAdjuster(Dictionary<string, double> Factors) : IPayBandCheckpointAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PayBandCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPayBandCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IPayBandCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandCheckpoint item, double valueValue)
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

public sealed record PercentPayBandCheckpointAdjuster(double Percent) : IPayBandCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayBandCheckpointAdjuster(double Minimum) : IPayBandCheckpointAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPayBandCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IPayBandCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pay band checkpoint steps in order and keeps a trace of each.</summary>
public sealed class PayBandCheckpointEngine(List<IPayBandCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandCheckpointEngine DefaultPayBandCheckpointEngine(Registry flags)
    {
        return new PayBandCheckpointEngine(
            new List<IPayBandCheckpointAdjuster>
            {
                new SegmentPayBandCheckpointAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.77, ["east"] = 1.13 }),
                new TieredPayBandCheckpointAdjuster(
                    new List<double> { 5675.0, 7087.0, 9899.0 },
                    new List<double> { 0.91, 1.07, 0.86 }),
                new PercentPayBandCheckpointAdjuster(-1.4),
                new MinimumPayBandCheckpointAdjuster(48.2),
                new FlaggedPayBandCheckpointAdjuster(flags, "pay-band-checkpoint-scheduled", 9.3),
            },
            new List<string>(),
            2.1,
            16426.8);
    }

    public double Run(PayBandCheckpoint item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
