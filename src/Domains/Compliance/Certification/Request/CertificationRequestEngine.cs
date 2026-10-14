namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification request calculation.</summary>
public interface ICertificationRequestModifier
{
    string Describe();
    double Adjust(CertificationRequest item, double valueValue);
}

public sealed record CertificationRequestQuantityOffsetAdjuster(double Weight) : ICertificationRequestModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(CertificationRequest item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record FlaggedCertificationRequestAdjuster(Registry Flags, string Flag, double Percent) : ICertificationRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleCertificationRequestAdjuster(Dictionary<string, double> Factors) : ICertificationRequestModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(CertificationRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the certification request steps in order and keeps a trace of each.</summary>
public sealed class CertificationRequestEngine(List<ICertificationRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationRequestEngine DefaultCertificationRequestEngine(Registry flags)
    {
        return new CertificationRequestEngine(
            new List<ICertificationRequestModifier>
            {
                new CertificationRequestQuantityOffsetAdjuster(1.19),
                new FlaggedCertificationRequestAdjuster(flags, "certification-request-regional", -3.3),
                new TitleCertificationRequestAdjuster(new Dictionary<string, double> { ["west"] = 0.84 }),
            },
            new List<string>(),
            4.2,
            3653.3);
    }

    public double Price(CertificationRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<CertificationRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
