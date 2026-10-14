namespace Acme.Workforce.Compliance.Attestation.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation assignment calculation.</summary>
public interface IAttestationAssignmentFactor
{
    string Describe();
    double Adjust(AttestationAssignment item, double valueValue);
}

public sealed record CapAttestationAssignmentAdjuster(double Cap) : IAttestationAssignmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAttestationAssignmentAdjuster(double Step) : IAttestationAssignmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AttestationAssignmentCreatedAtOffsetAdjuster(double Weight) : IAttestationAssignmentFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(AttestationAssignment item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record MinimumAttestationAssignmentAdjuster(double Minimum) : IAttestationAssignmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelAttestationAssignmentAdjuster(Dictionary<string, double> Factors) : IAttestationAssignmentFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AttestationAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAttestationAssignmentAdjuster(double Percent) : IAttestationAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the attestation assignment steps in order and keeps a trace of each.</summary>
public sealed class AttestationAssignmentEngine(List<IAttestationAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationAssignmentEngine DefaultAttestationAssignmentEngine()
    {
        return new AttestationAssignmentEngine(
            new List<IAttestationAssignmentFactor>
            {
                new CapAttestationAssignmentAdjuster(26471.0),
                new RoundingAttestationAssignmentAdjuster(0.25),
                new AttestationAssignmentCreatedAtOffsetAdjuster(1.48),
                new MinimumAttestationAssignmentAdjuster(13.9),
                new LabelAttestationAssignmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.1, ["central"] = 1.29 }),
                new PercentAttestationAssignmentAdjuster(-4.2),
            },
            new List<string>(),
            3.1,
            42178.2);
    }

    public double Compute(AttestationAssignment item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
