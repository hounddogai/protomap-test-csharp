namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification notice calculation.</summary>
public interface ICertificationNoticeAdjuster
{
    string Describe();
    double Adjust(CertificationNotice item, double valueValue);
}

public sealed record DepartmentCertificationNoticeAdjuster(Dictionary<string, double> Factors) : ICertificationNoticeAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(CertificationNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredCertificationNoticeAdjuster(List<double> Bounds, List<double> Rates) : ICertificationNoticeAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationNotice item, double valueValue)
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

public sealed record PercentCertificationNoticeAdjuster(double Percent) : ICertificationNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCertificationNoticeAdjuster(double Minimum) : ICertificationNoticeAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCertificationNoticeAdjuster(double Step) : ICertificationNoticeAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CertificationNoticeDiscountOffsetAdjuster(double Weight) : ICertificationNoticeAdjuster
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(CertificationNotice item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the certification notice steps in order and keeps a trace of each.</summary>
public sealed class CertificationNoticeEngine(List<ICertificationNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationNoticeEngine DefaultCertificationNoticeEngine()
    {
        return new CertificationNoticeEngine(
            new List<ICertificationNoticeAdjuster>
            {
                new DepartmentCertificationNoticeAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.83, ["east"] = 1.37, ["online"] = 1.03 }),
                new TieredCertificationNoticeAdjuster(
                    new List<double> { 315.0, 621.0, 8883.0 },
                    new List<double> { 0.82, 0.85, 1.04 }),
                new PercentCertificationNoticeAdjuster(-12.2),
                new MinimumCertificationNoticeAdjuster(46.0),
                new RoundingCertificationNoticeAdjuster(0.25),
                new CertificationNoticeDiscountOffsetAdjuster(1.07),
            },
            new List<string>(),
            4.8,
            7536.1);
    }

    public double Run(CertificationNotice item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
