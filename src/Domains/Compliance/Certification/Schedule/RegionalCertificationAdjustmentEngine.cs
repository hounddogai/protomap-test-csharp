namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional certification adjustment calculation.</summary>
public interface IRegionalCertificationAdjustmentStep
{
    string Describe();
    double Adjust(RegionalCertificationAdjustment item, double valueValue);
}

public sealed record MinimumRegionalCertificationAdjustmentAdjuster(double Minimum) : IRegionalCertificationAdjustmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalCertificationAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRegionalCertificationAdjustmentAdjuster(double Cap) : IRegionalCertificationAdjustmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalCertificationAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionalCertificationAdjustmentWeightOffsetAdjuster(double Weight) : IRegionalCertificationAdjustmentStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(RegionalCertificationAdjustment item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record PercentRegionalCertificationAdjustmentAdjuster(double Percent) : IRegionalCertificationAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalCertificationAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentRegionalCertificationAdjustmentAdjuster(Dictionary<string, double> Factors) : IRegionalCertificationAdjustmentStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(RegionalCertificationAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRegionalCertificationAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IRegionalCertificationAdjustmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalCertificationAdjustment item, double valueValue)
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

/// <summary>Runs the regional certification adjustment steps in order and keeps a trace of each.</summary>
public sealed class RegionalCertificationAdjustmentEngine(List<IRegionalCertificationAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalCertificationAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalCertificationAdjustmentEngine DefaultRegionalCertificationAdjustmentEngine()
    {
        return new RegionalCertificationAdjustmentEngine(
            new List<IRegionalCertificationAdjustmentStep>
            {
                new MinimumRegionalCertificationAdjustmentAdjuster(24.9),
                new CapRegionalCertificationAdjustmentAdjuster(6928.0),
                new RegionalCertificationAdjustmentWeightOffsetAdjuster(1.45),
                new PercentRegionalCertificationAdjustmentAdjuster(-13.4),
                new SegmentRegionalCertificationAdjustmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.73, ["central"] = 1.01 }),
                new TieredRegionalCertificationAdjustmentAdjuster(
                    new List<double> { 2243.0, 5275.0, 6215.0 },
                    new List<double> { 0.92, 1.14, 1.16 }),
            },
            new List<string>(),
            3.9,
            56480.4);
    }

    public double Evaluate(RegionalCertificationAdjustment item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalCertificationAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
