namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary pay band checkpoint calculation.</summary>
public interface IPrimaryPayBandCheckpointFactor
{
    string Describe();
    double Adjust(PrimaryPayBandCheckpoint item, double valueValue);
}

public sealed record PercentPrimaryPayBandCheckpointAdjuster(double Percent) : IPrimaryPayBandCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryPayBandCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelPrimaryPayBandCheckpointAdjuster(Dictionary<string, double> Factors) : IPrimaryPayBandCheckpointFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PrimaryPayBandCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPrimaryPayBandCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryPayBandCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryPayBandCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryPayBandCheckpointAdjuster(double Cap) : IPrimaryPayBandCheckpointFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryPayBandCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PrimaryPayBandCheckpointLeaveDaysOffsetAdjuster(double Weight) : IPrimaryPayBandCheckpointFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(PrimaryPayBandCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record MinimumPrimaryPayBandCheckpointAdjuster(double Minimum) : IPrimaryPayBandCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryPayBandCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary pay band checkpoint steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPayBandCheckpointEngine(List<IPrimaryPayBandCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPayBandCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPayBandCheckpointEngine DefaultPrimaryPayBandCheckpointEngine(Registry flags)
    {
        return new PrimaryPayBandCheckpointEngine(
            new List<IPrimaryPayBandCheckpointFactor>
            {
                new PercentPrimaryPayBandCheckpointAdjuster(-19.6),
                new LabelPrimaryPayBandCheckpointAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.87, ["east"] = 0.95 }),
                new FlaggedPrimaryPayBandCheckpointAdjuster(flags, "primary-pay-band-checkpoint-batched", 1.3),
                new CapPrimaryPayBandCheckpointAdjuster(33249.0),
                new PrimaryPayBandCheckpointLeaveDaysOffsetAdjuster(0.54),
                new MinimumPrimaryPayBandCheckpointAdjuster(20.8),
            },
            new List<string>(),
            3.1,
            75067.8);
    }

    public double Evaluate(PrimaryPayBandCheckpoint item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPayBandCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
