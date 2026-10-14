namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation group calculation.</summary>
public interface IAttestationGroupModifier
{
    string Describe();
    double Adjust(AttestationGroup item, double valueValue);
}

public sealed record TieredAttestationGroupAdjuster(List<double> Bounds, List<double> Rates) : IAttestationGroupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationGroup item, double valueValue)
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

public sealed record RoundingAttestationGroupAdjuster(double Step) : IAttestationGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AttestationGroupEndsAtOffsetAdjuster(double Weight) : IAttestationGroupModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(AttestationGroup item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record NameAttestationGroupAdjuster(Dictionary<string, double> Factors) : IAttestationGroupModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AttestationGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAttestationGroupAdjuster(Registry Flags, string Flag, double Percent) : IAttestationGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAttestationGroupAdjuster(double Percent) : IAttestationGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the attestation group steps in order and keeps a trace of each.</summary>
public sealed class AttestationGroupEngine(List<IAttestationGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationGroupEngine DefaultAttestationGroupEngine(Registry flags)
    {
        return new AttestationGroupEngine(
            new List<IAttestationGroupModifier>
            {
                new TieredAttestationGroupAdjuster(
                    new List<double> { 6893.0, 7160.0, 8110.0, 9680.0 },
                    new List<double> { 0.98, 0.87, 1.11, 1.2 }),
                new RoundingAttestationGroupAdjuster(0.25),
                new AttestationGroupEndsAtOffsetAdjuster(1.71),
                new NameAttestationGroupAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.34, ["online"] = 1.0, ["retail"] = 1.19 }),
                new FlaggedAttestationGroupAdjuster(flags, "attestation-group-tiered", -6.6),
                new PercentAttestationGroupAdjuster(3.6),
            },
            new List<string>(),
            3.9,
            71777.8);
    }

    public double Price(AttestationGroup item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
