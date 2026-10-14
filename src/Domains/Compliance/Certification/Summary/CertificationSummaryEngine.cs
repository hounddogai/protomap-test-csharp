namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification summary calculation.</summary>
public interface ICertificationSummaryModifier
{
    string Describe();
    double Adjust(CertificationSummary item, double valueValue);
}

public sealed record CapCertificationSummaryAdjuster(double Cap) : ICertificationSummaryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCertificationSummaryAdjuster(Registry Flags, string Flag, double Percent) : ICertificationSummaryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CertificationSummaryMarginOffsetAdjuster(double Weight) : ICertificationSummaryModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(CertificationSummary item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the certification summary steps in order and keeps a trace of each.</summary>
public sealed class CertificationSummaryEngine(List<ICertificationSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationSummaryEngine DefaultCertificationSummaryEngine(Registry flags)
    {
        return new CertificationSummaryEngine(
            new List<ICertificationSummaryModifier>
            {
                new CapCertificationSummaryAdjuster(20643.0),
                new FlaggedCertificationSummaryAdjuster(flags, "certification-summary-batched", -0.8),
                new CertificationSummaryMarginOffsetAdjuster(1.62),
            },
            new List<string>(),
            4.6,
            63620.0);
    }

    public double Compute(CertificationSummary item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
