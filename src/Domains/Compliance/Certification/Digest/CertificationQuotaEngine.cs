namespace Acme.Workforce.Compliance.Certification.Digest;

/// <summary>One step of the certification quota calculation.</summary>
public interface ICertificationQuotaAdjuster
{
    string Describe();
    double Adjust(CertificationQuota item, double valueValue);
}

public sealed record CertificationQuotaBudgetOffsetAdjuster(double Weight) : ICertificationQuotaAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(CertificationQuota item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record PercentCertificationQuotaAdjuster(double Percent) : ICertificationQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCertificationQuotaAdjuster(List<double> Bounds, List<double> Rates) : ICertificationQuotaAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationQuota item, double valueValue)
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

public sealed record MinimumCertificationQuotaAdjuster(double Minimum) : ICertificationQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the certification quota steps in order and keeps a trace of each.</summary>
public sealed class CertificationQuotaEngine(List<ICertificationQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationQuotaEngine DefaultCertificationQuotaEngine()
    {
        return new CertificationQuotaEngine(
            new List<ICertificationQuotaAdjuster>
            {
                new CertificationQuotaBudgetOffsetAdjuster(1.33),
                new PercentCertificationQuotaAdjuster(3.9),
                new TieredCertificationQuotaAdjuster(
                    new List<double> { 1704.0, 4300.0, 5619.0 },
                    new List<double> { 1.02, 1.15, 1.12 }),
                new MinimumCertificationQuotaAdjuster(29.8),
            },
            new List<string>(),
            0.3,
            37475.0);
    }

    public double Run(CertificationQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<CertificationQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
