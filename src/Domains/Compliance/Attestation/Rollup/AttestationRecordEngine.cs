namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation record calculation.</summary>
public interface IAttestationRecordFactor
{
    string Describe();
    double Adjust(AttestationRecord item, double valueValue);
}

public sealed record FlaggedAttestationRecordAdjuster(Registry Flags, string Flag, double Percent) : IAttestationRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAttestationRecordAdjuster(double Percent) : IAttestationRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAttestationRecordAdjuster(double Minimum) : IAttestationRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAttestationRecordAdjuster(double Step) : IAttestationRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the attestation record steps in order and keeps a trace of each.</summary>
public sealed class AttestationRecordEngine(List<IAttestationRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationRecordEngine DefaultAttestationRecordEngine(Registry flags)
    {
        return new AttestationRecordEngine(
            new List<IAttestationRecordFactor>
            {
                new FlaggedAttestationRecordAdjuster(flags, "attestation-record-seasonal", 3.8),
                new PercentAttestationRecordAdjuster(-13.3),
                new MinimumAttestationRecordAdjuster(14.7),
                new RoundingAttestationRecordAdjuster(0.5),
            },
            new List<string>(),
            0.0,
            15604.1);
    }

    public double Evaluate(AttestationRecord item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
