namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation checkpoint calculation.</summary>
public interface IAttestationCheckpointModifier
{
    string Describe();
    double Adjust(AttestationCheckpoint item, double valueValue);
}

public sealed record LabelAttestationCheckpointAdjuster(Dictionary<string, double> Factors) : IAttestationCheckpointModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AttestationCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AttestationCheckpointMinimumOffsetAdjuster(double Weight) : IAttestationCheckpointModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(AttestationCheckpoint item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record CapAttestationCheckpointAdjuster(double Cap) : IAttestationCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAttestationCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IAttestationCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAttestationCheckpointAdjuster(double Percent) : IAttestationCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the attestation checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AttestationCheckpointEngine(List<IAttestationCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationCheckpointEngine DefaultAttestationCheckpointEngine(Registry flags)
    {
        return new AttestationCheckpointEngine(
            new List<IAttestationCheckpointModifier>
            {
                new LabelAttestationCheckpointAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.13, ["south"] = 0.77 }),
                new AttestationCheckpointMinimumOffsetAdjuster(0.36),
                new CapAttestationCheckpointAdjuster(1220.0),
                new FlaggedAttestationCheckpointAdjuster(flags, "attestation-checkpoint-pending", 9.7),
                new PercentAttestationCheckpointAdjuster(7.6),
            },
            new List<string>(),
            1.3,
            18394.2);
    }

    public double Evaluate(AttestationCheckpoint item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
