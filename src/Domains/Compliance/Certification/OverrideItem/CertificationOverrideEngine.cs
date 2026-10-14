namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification override calculation.</summary>
public interface ICertificationOverrideAdjuster
{
    string Describe();
    double Adjust(CertificationOverride item, double valueValue);
}

public sealed record SourceCertificationOverrideAdjuster(Dictionary<string, double> Factors) : ICertificationOverrideAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(CertificationOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCertificationOverrideAdjuster(double Cap) : ICertificationOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCertificationOverrideAdjuster(Registry Flags, string Flag, double Percent) : ICertificationOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CertificationOverrideSequenceOffsetAdjuster(double Weight) : ICertificationOverrideAdjuster
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(CertificationOverride item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record RoundingCertificationOverrideAdjuster(double Step) : ICertificationOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the certification override steps in order and keeps a trace of each.</summary>
public sealed class CertificationOverrideEngine(List<ICertificationOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationOverrideEngine DefaultCertificationOverrideEngine(Registry flags)
    {
        return new CertificationOverrideEngine(
            new List<ICertificationOverrideAdjuster>
            {
                new SourceCertificationOverrideAdjuster(new Dictionary<string, double> { ["north"] = 0.72 }),
                new CapCertificationOverrideAdjuster(12412.0),
                new FlaggedCertificationOverrideAdjuster(flags, "certification-override-strict", 1.8),
                new CertificationOverrideSequenceOffsetAdjuster(1.1),
                new RoundingCertificationOverrideAdjuster(0.1),
            },
            new List<string>(),
            4.0,
            60918.7);
    }

    public double Price(CertificationOverride item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
