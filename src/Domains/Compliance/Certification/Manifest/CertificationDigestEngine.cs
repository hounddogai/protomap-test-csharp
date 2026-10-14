namespace Acme.Workforce.Compliance.Certification.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification digest calculation.</summary>
public interface ICertificationDigestAdjuster
{
    string Describe();
    double Adjust(CertificationDigest item, double valueValue);
}

public sealed record RoundingCertificationDigestAdjuster(double Step) : ICertificationDigestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CertificationDigestCostOffsetAdjuster(double Weight) : ICertificationDigestAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(CertificationDigest item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record MinimumCertificationDigestAdjuster(double Minimum) : ICertificationDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the certification digest steps in order and keeps a trace of each.</summary>
public sealed class CertificationDigestEngine(List<ICertificationDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationDigestEngine DefaultCertificationDigestEngine()
    {
        return new CertificationDigestEngine(
            new List<ICertificationDigestAdjuster>
            {
                new RoundingCertificationDigestAdjuster(0.05),
                new CertificationDigestCostOffsetAdjuster(0.23),
                new MinimumCertificationDigestAdjuster(12.3),
            },
            new List<string>(),
            0.5,
            63340.5);
    }

    public double Evaluate(CertificationDigest item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
