namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification adjustment calculation.</summary>
public interface ICertificationAdjustmentModifier
{
    string Describe();
    double Adjust(CertificationAdjustment item, double valueValue);
}

public sealed record MinimumCertificationAdjustmentAdjuster(double Minimum) : ICertificationAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCertificationAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ICertificationAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationAdjustment item, double valueValue)
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

public sealed record LabelCertificationAdjustmentAdjuster(Dictionary<string, double> Factors) : ICertificationAdjustmentModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CertificationAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CertificationAdjustmentThresholdOffsetAdjuster(double Weight) : ICertificationAdjustmentModifier
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(CertificationAdjustment item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the certification adjustment steps in order and keeps a trace of each.</summary>
public sealed class CertificationAdjustmentEngine(List<ICertificationAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationAdjustmentEngine DefaultCertificationAdjustmentEngine()
    {
        return new CertificationAdjustmentEngine(
            new List<ICertificationAdjustmentModifier>
            {
                new MinimumCertificationAdjustmentAdjuster(36.6),
                new TieredCertificationAdjustmentAdjuster(
                    new List<double> { 4116.0, 8503.0 },
                    new List<double> { 1.01, 1.2 }),
                new LabelCertificationAdjustmentAdjuster(new Dictionary<string, double> { ["online"] = 0.89 }),
                new CertificationAdjustmentThresholdOffsetAdjuster(0.73),
            },
            new List<string>(),
            2.6,
            66679.5);
    }

    public double Price(CertificationAdjustment item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
