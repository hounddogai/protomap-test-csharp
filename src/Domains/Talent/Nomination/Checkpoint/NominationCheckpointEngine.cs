namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination checkpoint calculation.</summary>
public interface INominationCheckpointFactor
{
    string Describe();
    double Adjust(NominationCheckpoint item, double valueValue);
}

public sealed record PercentNominationCheckpointAdjuster(double Percent) : INominationCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumNominationCheckpointAdjuster(double Minimum) : INominationCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NominationCheckpointCapacityOffsetAdjuster(double Weight) : INominationCheckpointFactor
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(NominationCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record FlaggedNominationCheckpointAdjuster(Registry Flags, string Flag, double Percent) : INominationCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the nomination checkpoint steps in order and keeps a trace of each.</summary>
public sealed class NominationCheckpointEngine(List<INominationCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationCheckpointEngine DefaultNominationCheckpointEngine(Registry flags)
    {
        return new NominationCheckpointEngine(
            new List<INominationCheckpointFactor>
            {
                new PercentNominationCheckpointAdjuster(-11.1),
                new MinimumNominationCheckpointAdjuster(35.0),
                new NominationCheckpointCapacityOffsetAdjuster(0.14),
                new FlaggedNominationCheckpointAdjuster(flags, "nomination-checkpoint-batched", 4.5),
            },
            new List<string>(),
            1.2,
            17209.9);
    }

    public double Compute(NominationCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<NominationCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
