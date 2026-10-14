namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation segment calculation.</summary>
public interface IAttestationSegmentAdjuster
{
    string Describe();
    double Adjust(AttestationSegment item, double valueValue);
}

public sealed record RoundingAttestationSegmentAdjuster(double Step) : IAttestationSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAttestationSegmentAdjuster(double Cap) : IAttestationSegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameAttestationSegmentAdjuster(Dictionary<string, double> Factors) : IAttestationSegmentAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AttestationSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAttestationSegmentAdjuster(double Percent) : IAttestationSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAttestationSegmentAdjuster(double Minimum) : IAttestationSegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the attestation segment steps in order and keeps a trace of each.</summary>
public sealed class AttestationSegmentEngine(List<IAttestationSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationSegmentEngine DefaultAttestationSegmentEngine()
    {
        return new AttestationSegmentEngine(
            new List<IAttestationSegmentAdjuster>
            {
                new RoundingAttestationSegmentAdjuster(0.1),
                new CapAttestationSegmentAdjuster(38998.0),
                new NameAttestationSegmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.28, ["east"] = 1.25, ["north"] = 1.31 }),
                new PercentAttestationSegmentAdjuster(18.5),
                new MinimumAttestationSegmentAdjuster(6.5),
            },
            new List<string>(),
            0.2,
            76026.5);
    }

    public double Compute(AttestationSegment item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
