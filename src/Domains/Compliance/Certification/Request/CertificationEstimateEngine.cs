namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification estimate calculation.</summary>
public interface ICertificationEstimateAdjuster
{
    string Describe();
    double Adjust(CertificationEstimate item, double valueValue);
}

public sealed record PercentCertificationEstimateAdjuster(double Percent) : ICertificationEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCertificationEstimateAdjuster(double Minimum) : ICertificationEstimateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCertificationEstimateAdjuster(List<double> Bounds, List<double> Rates) : ICertificationEstimateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationEstimate item, double valueValue)
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

/// <summary>Runs the certification estimate steps in order and keeps a trace of each.</summary>
public sealed class CertificationEstimateEngine(List<ICertificationEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationEstimateEngine DefaultCertificationEstimateEngine()
    {
        return new CertificationEstimateEngine(
            new List<ICertificationEstimateAdjuster>
            {
                new PercentCertificationEstimateAdjuster(-24.5),
                new MinimumCertificationEstimateAdjuster(34.6),
                new TieredCertificationEstimateAdjuster(
                    new List<double> { 3045.0, 7127.0, 9374.0 },
                    new List<double> { 0.89, 0.94, 0.82 }),
            },
            new List<string>(),
            3.4,
            36450.2);
    }

    public double Price(CertificationEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<CertificationEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
