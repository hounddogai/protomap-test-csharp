namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation batch calculation.</summary>
public interface IAttestationBatchAdjuster
{
    string Describe();
    double Adjust(AttestationBatch item, double valueValue);
}

public sealed record CapAttestationBatchAdjuster(double Cap) : IAttestationBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AttestationBatchTenureMonthsOffsetAdjuster(double Weight) : IAttestationBatchAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(AttestationBatch item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record FlaggedAttestationBatchAdjuster(Registry Flags, string Flag, double Percent) : IAttestationBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAttestationBatchAdjuster(double Percent) : IAttestationBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAttestationBatchAdjuster(double Minimum) : IAttestationBatchAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAttestationBatchAdjuster(double Step) : IAttestationBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the attestation batch steps in order and keeps a trace of each.</summary>
public sealed class AttestationBatchEngine(List<IAttestationBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationBatchEngine DefaultAttestationBatchEngine(Registry flags)
    {
        return new AttestationBatchEngine(
            new List<IAttestationBatchAdjuster>
            {
                new CapAttestationBatchAdjuster(11874.0),
                new AttestationBatchTenureMonthsOffsetAdjuster(0.99),
                new FlaggedAttestationBatchAdjuster(flags, "attestation-batch-seasonal", -1.3),
                new PercentAttestationBatchAdjuster(9.6),
                new MinimumAttestationBatchAdjuster(11.4),
                new RoundingAttestationBatchAdjuster(5.0),
            },
            new List<string>(),
            4.4,
            14211.9);
    }

    public double Run(AttestationBatch item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
