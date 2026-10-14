namespace Acme.Workforce.Orgdesign.Position.PositionEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending position estimate calculation.</summary>
public interface IPendingPositionEstimateFactor
{
    string Describe();
    double Adjust(PendingPositionEstimate item, double valueValue);
}

public sealed record RoundingPendingPositionEstimateAdjuster(double Step) : IPendingPositionEstimateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingPositionEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPendingPositionEstimateAdjuster(double Minimum) : IPendingPositionEstimateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingPositionEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPendingPositionEstimateAdjuster(double Cap) : IPendingPositionEstimateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingPositionEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPendingPositionEstimateAdjuster(Registry Flags, string Flag, double Percent) : IPendingPositionEstimateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingPositionEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingPositionEstimateAdjuster(List<double> Bounds, List<double> Rates) : IPendingPositionEstimateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingPositionEstimate item, double valueValue)
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

public sealed record PendingPositionEstimateThresholdOffsetAdjuster(double Weight) : IPendingPositionEstimateFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(PendingPositionEstimate item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the pending position estimate steps in order and keeps a trace of each.</summary>
public sealed class PendingPositionEstimateEngine(List<IPendingPositionEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPositionEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPositionEstimateEngine DefaultPendingPositionEstimateEngine(Registry flags)
    {
        return new PendingPositionEstimateEngine(
            new List<IPendingPositionEstimateFactor>
            {
                new RoundingPendingPositionEstimateAdjuster(5.0),
                new MinimumPendingPositionEstimateAdjuster(36.8),
                new CapPendingPositionEstimateAdjuster(4756.0),
                new FlaggedPendingPositionEstimateAdjuster(flags, "pending-position-estimate-strict", -8.6),
                new TieredPendingPositionEstimateAdjuster(
                    new List<double> { 8624.0, 9544.0 },
                    new List<double> { 1.05, 1.03 }),
                new PendingPositionEstimateThresholdOffsetAdjuster(1.11),
            },
            new List<string>(),
            2.4,
            62235.9);
    }

    public double Run(PendingPositionEstimate item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PendingPositionEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
