namespace Acme.Workforce.Compliance.Attestation.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation audit calculation.</summary>
public interface IAttestationAuditAdjuster
{
    string Describe();
    double Adjust(AttestationAudit item, double valueValue);
}

public sealed record PercentAttestationAuditAdjuster(double Percent) : IAttestationAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAttestationAuditAdjuster(double Cap) : IAttestationAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAttestationAuditAdjuster(double Minimum) : IAttestationAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAttestationAuditAdjuster(double Step) : IAttestationAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the attestation audit steps in order and keeps a trace of each.</summary>
public sealed class AttestationAuditEngine(List<IAttestationAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationAuditEngine DefaultAttestationAuditEngine()
    {
        return new AttestationAuditEngine(
            new List<IAttestationAuditAdjuster>
            {
                new PercentAttestationAuditAdjuster(-13.9),
                new CapAttestationAuditAdjuster(21960.0),
                new MinimumAttestationAuditAdjuster(5.2),
                new RoundingAttestationAuditAdjuster(0.05),
            },
            new List<string>(),
            3.3,
            56997.9);
    }

    public double Evaluate(AttestationAudit item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
