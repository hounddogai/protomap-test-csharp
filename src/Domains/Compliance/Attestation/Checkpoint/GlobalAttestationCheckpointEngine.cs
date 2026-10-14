namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the global attestation checkpoint calculation.</summary>
public interface IGlobalAttestationCheckpointModifier
{
    string Describe();
    double Adjust(GlobalAttestationCheckpoint item, double valueValue);
}

public sealed record MinimumGlobalAttestationCheckpointAdjuster(double Minimum) : IGlobalAttestationCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalAttestationCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredGlobalAttestationCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IGlobalAttestationCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalAttestationCheckpoint item, double valueValue)
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

public sealed record PercentGlobalAttestationCheckpointAdjuster(double Percent) : IGlobalAttestationCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalAttestationCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalAttestationCheckpointAttemptsOffsetAdjuster(double Weight) : IGlobalAttestationCheckpointModifier
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(GlobalAttestationCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record FlaggedGlobalAttestationCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IGlobalAttestationCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalAttestationCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global attestation checkpoint steps in order and keeps a trace of each.</summary>
public sealed class GlobalAttestationCheckpointEngine(List<IGlobalAttestationCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAttestationCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAttestationCheckpointEngine DefaultGlobalAttestationCheckpointEngine(Registry flags)
    {
        return new GlobalAttestationCheckpointEngine(
            new List<IGlobalAttestationCheckpointModifier>
            {
                new MinimumGlobalAttestationCheckpointAdjuster(45.1),
                new TieredGlobalAttestationCheckpointAdjuster(
                    new List<double> { 110.0, 1094.0, 4330.0, 9324.0 },
                    new List<double> { 1.03, 1.15, 0.84, 1.15 }),
                new PercentGlobalAttestationCheckpointAdjuster(6.3),
                new GlobalAttestationCheckpointAttemptsOffsetAdjuster(0.36),
                new FlaggedGlobalAttestationCheckpointAdjuster(flags, "global-attestation-checkpoint-batched", 1.0),
            },
            new List<string>(),
            1.7,
            90099.3);
    }

    public double Price(GlobalAttestationCheckpoint item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAttestationCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
