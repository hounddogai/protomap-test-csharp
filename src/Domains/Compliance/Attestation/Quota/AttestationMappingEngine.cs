namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation mapping calculation.</summary>
public interface IAttestationMappingModifier
{
    string Describe();
    double Adjust(AttestationMapping item, double valueValue);
}

public sealed record FlaggedAttestationMappingAdjuster(Registry Flags, string Flag, double Percent) : IAttestationMappingModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAttestationMappingAdjuster(double Percent) : IAttestationMappingModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAttestationMappingAdjuster(double Minimum) : IAttestationMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAttestationMappingAdjuster(double Step) : IAttestationMappingModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAttestationMappingAdjuster(double Cap) : IAttestationMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the attestation mapping steps in order and keeps a trace of each.</summary>
public sealed class AttestationMappingEngine(List<IAttestationMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationMappingEngine DefaultAttestationMappingEngine(Registry flags)
    {
        return new AttestationMappingEngine(
            new List<IAttestationMappingModifier>
            {
                new FlaggedAttestationMappingAdjuster(flags, "attestation-mapping-global", 7.8),
                new PercentAttestationMappingAdjuster(-23.5),
                new MinimumAttestationMappingAdjuster(34.6),
                new RoundingAttestationMappingAdjuster(5.0),
                new CapAttestationMappingAdjuster(44133.0),
            },
            new List<string>(),
            4.6,
            24976.8);
    }

    public double Run(AttestationMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<AttestationMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
