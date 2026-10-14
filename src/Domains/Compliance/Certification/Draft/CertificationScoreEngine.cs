namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification score calculation.</summary>
public interface ICertificationScoreAdjuster
{
    string Describe();
    double Adjust(CertificationScore item, double valueValue);
}

public sealed record PercentCertificationScoreAdjuster(double Percent) : ICertificationScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCertificationScoreAdjuster(double Step) : ICertificationScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCertificationScoreAdjuster(List<double> Bounds, List<double> Rates) : ICertificationScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationScore item, double valueValue)
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

public sealed record ChannelCertificationScoreAdjuster(Dictionary<string, double> Factors) : ICertificationScoreAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CertificationScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCertificationScoreAdjuster(double Cap) : ICertificationScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumCertificationScoreAdjuster(double Minimum) : ICertificationScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the certification score steps in order and keeps a trace of each.</summary>
public sealed class CertificationScoreEngine(List<ICertificationScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationScoreEngine DefaultCertificationScoreEngine()
    {
        return new CertificationScoreEngine(
            new List<ICertificationScoreAdjuster>
            {
                new PercentCertificationScoreAdjuster(-4.6),
                new RoundingCertificationScoreAdjuster(0.05),
                new TieredCertificationScoreAdjuster(
                    new List<double> { 720.0, 2914.0, 5734.0, 9530.0 },
                    new List<double> { 1.14, 0.83, 0.87, 0.87 }),
                new ChannelCertificationScoreAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.29, ["central"] = 1.31 }),
                new CapCertificationScoreAdjuster(12554.0),
                new MinimumCertificationScoreAdjuster(47.5),
            },
            new List<string>(),
            1.6,
            97242.8);
    }

    public double Compute(CertificationScore item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
