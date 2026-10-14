namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification milestone calculation.</summary>
public interface ICertificationMilestoneFactor
{
    string Describe();
    double Adjust(CertificationMilestone item, double valueValue);
}

public sealed record SourceCertificationMilestoneAdjuster(Dictionary<string, double> Factors) : ICertificationMilestoneFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(CertificationMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CertificationMilestoneMinimumOffsetAdjuster(double Weight) : ICertificationMilestoneFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(CertificationMilestone item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record CapCertificationMilestoneAdjuster(double Cap) : ICertificationMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentCertificationMilestoneAdjuster(double Percent) : ICertificationMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCertificationMilestoneAdjuster(Registry Flags, string Flag, double Percent) : ICertificationMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the certification milestone steps in order and keeps a trace of each.</summary>
public sealed class CertificationMilestoneEngine(List<ICertificationMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationMilestoneEngine DefaultCertificationMilestoneEngine(Registry flags)
    {
        return new CertificationMilestoneEngine(
            new List<ICertificationMilestoneFactor>
            {
                new SourceCertificationMilestoneAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.7, ["south"] = 1.22 }),
                new CertificationMilestoneMinimumOffsetAdjuster(0.3),
                new CapCertificationMilestoneAdjuster(4344.0),
                new PercentCertificationMilestoneAdjuster(-16.4),
                new FlaggedCertificationMilestoneAdjuster(flags, "certification-milestone-seasonal", -5.0),
            },
            new List<string>(),
            4.5,
            16660.5);
    }

    public double Run(CertificationMilestone item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
