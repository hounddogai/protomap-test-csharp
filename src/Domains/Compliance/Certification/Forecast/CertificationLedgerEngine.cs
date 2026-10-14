namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification ledger calculation.</summary>
public interface ICertificationLedgerModifier
{
    string Describe();
    double Adjust(CertificationLedger item, double valueValue);
}

public sealed record RoundingCertificationLedgerAdjuster(double Step) : ICertificationLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CertificationLedgerUpdatedAtOffsetAdjuster(double Weight) : ICertificationLedgerModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(CertificationLedger item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record CategoryCertificationLedgerAdjuster(Dictionary<string, double> Factors) : ICertificationLedgerModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(CertificationLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumCertificationLedgerAdjuster(double Minimum) : ICertificationLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCertificationLedgerAdjuster(List<double> Bounds, List<double> Rates) : ICertificationLedgerModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationLedger item, double valueValue)
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

public sealed record CapCertificationLedgerAdjuster(double Cap) : ICertificationLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the certification ledger steps in order and keeps a trace of each.</summary>
public sealed class CertificationLedgerEngine(List<ICertificationLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationLedgerEngine DefaultCertificationLedgerEngine()
    {
        return new CertificationLedgerEngine(
            new List<ICertificationLedgerModifier>
            {
                new RoundingCertificationLedgerAdjuster(0.1),
                new CertificationLedgerUpdatedAtOffsetAdjuster(1.91),
                new CategoryCertificationLedgerAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.72, ["east"] = 1.25 }),
                new MinimumCertificationLedgerAdjuster(12.1),
                new TieredCertificationLedgerAdjuster(
                    new List<double> { 7004.0, 9441.0 },
                    new List<double> { 1.14, 0.97 }),
                new CapCertificationLedgerAdjuster(3581.0),
            },
            new List<string>(),
            3.9,
            56347.5);
    }

    public double Price(CertificationLedger item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
