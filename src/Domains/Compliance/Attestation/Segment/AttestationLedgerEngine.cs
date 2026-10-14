namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation ledger calculation.</summary>
public interface IAttestationLedgerFactor
{
    string Describe();
    double Adjust(AttestationLedger item, double valueValue);
}

public sealed record TieredAttestationLedgerAdjuster(List<double> Bounds, List<double> Rates) : IAttestationLedgerFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationLedger item, double valueValue)
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

public sealed record RoundingAttestationLedgerAdjuster(double Step) : IAttestationLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AttestationLedgerQuantityOffsetAdjuster(double Weight) : IAttestationLedgerFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(AttestationLedger item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record MinimumAttestationLedgerAdjuster(double Minimum) : IAttestationLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the attestation ledger steps in order and keeps a trace of each.</summary>
public sealed class AttestationLedgerEngine(List<IAttestationLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationLedgerEngine DefaultAttestationLedgerEngine()
    {
        return new AttestationLedgerEngine(
            new List<IAttestationLedgerFactor>
            {
                new TieredAttestationLedgerAdjuster(
                    new List<double> { 5575.0, 6812.0, 7686.0, 8676.0 },
                    new List<double> { 1.15, 1.08, 0.81, 1.06 }),
                new RoundingAttestationLedgerAdjuster(0.25),
                new AttestationLedgerQuantityOffsetAdjuster(0.06),
                new MinimumAttestationLedgerAdjuster(17.7),
            },
            new List<string>(),
            3.5,
            89151.6);
    }

    public double Evaluate(AttestationLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<AttestationLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
