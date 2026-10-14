namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation rollup calculation.</summary>
public interface IAttestationRollupModifier
{
    string Describe();
    double Adjust(AttestationRollup item, double valueValue);
}

public sealed record RoundingAttestationRollupAdjuster(double Step) : IAttestationRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAttestationRollupAdjuster(Registry Flags, string Flag, double Percent) : IAttestationRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAttestationRollupAdjuster(List<double> Bounds, List<double> Rates) : IAttestationRollupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationRollup item, double valueValue)
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

public sealed record MinimumAttestationRollupAdjuster(double Minimum) : IAttestationRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the attestation rollup steps in order and keeps a trace of each.</summary>
public sealed class AttestationRollupEngine(List<IAttestationRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationRollupEngine DefaultAttestationRollupEngine(Registry flags)
    {
        return new AttestationRollupEngine(
            new List<IAttestationRollupModifier>
            {
                new RoundingAttestationRollupAdjuster(0.5),
                new FlaggedAttestationRollupAdjuster(flags, "attestation-rollup-batched", -9.1),
                new TieredAttestationRollupAdjuster(
                    new List<double> { 408.0, 9673.0 },
                    new List<double> { 0.98, 0.99 }),
                new MinimumAttestationRollupAdjuster(32.8),
            },
            new List<string>(),
            2.8,
            94318.3);
    }

    public double Price(AttestationRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<AttestationRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
