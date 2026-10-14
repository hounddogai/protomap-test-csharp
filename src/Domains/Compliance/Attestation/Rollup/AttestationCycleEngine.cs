namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation cycle calculation.</summary>
public interface IAttestationCycleModifier
{
    string Describe();
    double Adjust(AttestationCycle item, double valueValue);
}

public sealed record FlaggedAttestationCycleAdjuster(Registry Flags, string Flag, double Percent) : IAttestationCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAttestationCycleAdjuster(double Cap) : IAttestationCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAttestationCycleAdjuster(List<double> Bounds, List<double> Rates) : IAttestationCycleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationCycle item, double valueValue)
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

public sealed record MinimumAttestationCycleAdjuster(double Minimum) : IAttestationCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AttestationCycleRatioOffsetAdjuster(double Weight) : IAttestationCycleModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(AttestationCycle item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record RoundingAttestationCycleAdjuster(double Step) : IAttestationCycleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the attestation cycle steps in order and keeps a trace of each.</summary>
public sealed class AttestationCycleEngine(List<IAttestationCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationCycleEngine DefaultAttestationCycleEngine(Registry flags)
    {
        return new AttestationCycleEngine(
            new List<IAttestationCycleModifier>
            {
                new FlaggedAttestationCycleAdjuster(flags, "attestation-cycle-fallback", -6.5),
                new CapAttestationCycleAdjuster(7370.0),
                new TieredAttestationCycleAdjuster(
                    new List<double> { 192.0, 782.0, 6980.0 },
                    new List<double> { 1.01, 0.97, 1.11 }),
                new MinimumAttestationCycleAdjuster(18.3),
                new AttestationCycleRatioOffsetAdjuster(1.71),
                new RoundingAttestationCycleAdjuster(0.25),
            },
            new List<string>(),
            3.1,
            61232.5);
    }

    public double Run(AttestationCycle item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
