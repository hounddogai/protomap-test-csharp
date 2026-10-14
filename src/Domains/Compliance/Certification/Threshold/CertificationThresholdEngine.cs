namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification threshold calculation.</summary>
public interface ICertificationThresholdAdjuster
{
    string Describe();
    double Adjust(CertificationThreshold item, double valueValue);
}

public sealed record TieredCertificationThresholdAdjuster(List<double> Bounds, List<double> Rates) : ICertificationThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationThreshold item, double valueValue)
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

public sealed record SourceCertificationThresholdAdjuster(Dictionary<string, double> Factors) : ICertificationThresholdAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(CertificationThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumCertificationThresholdAdjuster(double Minimum) : ICertificationThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the certification threshold steps in order and keeps a trace of each.</summary>
public sealed class CertificationThresholdEngine(List<ICertificationThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationThresholdEngine DefaultCertificationThresholdEngine()
    {
        return new CertificationThresholdEngine(
            new List<ICertificationThresholdAdjuster>
            {
                new TieredCertificationThresholdAdjuster(
                    new List<double> { 463.0, 1233.0 },
                    new List<double> { 1.18, 1.06 }),
                new SourceCertificationThresholdAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.26, ["south"] = 1.1, ["east"] = 1.18 }),
                new MinimumCertificationThresholdAdjuster(28.8),
            },
            new List<string>(),
            4.5,
            54130.6);
    }

    public double Evaluate(CertificationThreshold item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
