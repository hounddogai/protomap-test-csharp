namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered certification summary calculation.</summary>
public interface ITieredCertificationSummaryStep
{
    string Describe();
    double Adjust(TieredCertificationSummary item, double valueValue);
}

public sealed record CapTieredCertificationSummaryAdjuster(double Cap) : ITieredCertificationSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredCertificationSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTieredCertificationSummaryAdjuster(List<double> Bounds, List<double> Rates) : ITieredCertificationSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredCertificationSummary item, double valueValue)
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

public sealed record TieredCertificationSummaryMaximumOffsetAdjuster(double Weight) : ITieredCertificationSummaryStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(TieredCertificationSummary item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record PercentTieredCertificationSummaryAdjuster(double Percent) : ITieredCertificationSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredCertificationSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered certification summary steps in order and keeps a trace of each.</summary>
public sealed class TieredCertificationSummaryEngine(List<ITieredCertificationSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredCertificationSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredCertificationSummaryEngine DefaultTieredCertificationSummaryEngine()
    {
        return new TieredCertificationSummaryEngine(
            new List<ITieredCertificationSummaryStep>
            {
                new CapTieredCertificationSummaryAdjuster(30889.0),
                new TieredTieredCertificationSummaryAdjuster(
                    new List<double> { 8971.0, 8978.0 },
                    new List<double> { 0.84, 0.95 }),
                new TieredCertificationSummaryMaximumOffsetAdjuster(0.39),
                new PercentTieredCertificationSummaryAdjuster(-17.2),
            },
            new List<string>(),
            2.4,
            49022.6);
    }

    public double Price(TieredCertificationSummary item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredCertificationSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
