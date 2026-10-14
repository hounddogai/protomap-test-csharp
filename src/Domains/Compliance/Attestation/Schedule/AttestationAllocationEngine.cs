namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation allocation calculation.</summary>
public interface IAttestationAllocationModifier
{
    string Describe();
    double Adjust(AttestationAllocation item, double valueValue);
}

public sealed record RoundingAttestationAllocationAdjuster(double Step) : IAttestationAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAttestationAllocationAdjuster(double Percent) : IAttestationAllocationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAttestationAllocationAdjuster(double Minimum) : IAttestationAllocationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AttestationAllocationFteOffsetAdjuster(double Weight) : IAttestationAllocationModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(AttestationAllocation item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record CapAttestationAllocationAdjuster(double Cap) : IAttestationAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the attestation allocation steps in order and keeps a trace of each.</summary>
public sealed class AttestationAllocationEngine(List<IAttestationAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationAllocationEngine DefaultAttestationAllocationEngine()
    {
        return new AttestationAllocationEngine(
            new List<IAttestationAllocationModifier>
            {
                new RoundingAttestationAllocationAdjuster(0.05),
                new PercentAttestationAllocationAdjuster(9.5),
                new MinimumAttestationAllocationAdjuster(7.9),
                new AttestationAllocationFteOffsetAdjuster(0.65),
                new CapAttestationAllocationAdjuster(11602.0),
            },
            new List<string>(),
            4.8,
            53490.7);
    }

    public double Compute(AttestationAllocation item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
