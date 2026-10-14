namespace Acme.Workforce.Compliance.Certification.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification group calculation.</summary>
public interface ICertificationGroupAdjuster
{
    string Describe();
    double Adjust(CertificationGroup item, double valueValue);
}

public sealed record CertificationGroupRetriesOffsetAdjuster(double Weight) : ICertificationGroupAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(CertificationGroup item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record MinimumCertificationGroupAdjuster(double Minimum) : ICertificationGroupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameCertificationGroupAdjuster(Dictionary<string, double> Factors) : ICertificationGroupAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(CertificationGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingCertificationGroupAdjuster(double Step) : ICertificationGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCertificationGroupAdjuster(double Percent) : ICertificationGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCertificationGroupAdjuster(double Cap) : ICertificationGroupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the certification group steps in order and keeps a trace of each.</summary>
public sealed class CertificationGroupEngine(List<ICertificationGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationGroupEngine DefaultCertificationGroupEngine()
    {
        return new CertificationGroupEngine(
            new List<ICertificationGroupAdjuster>
            {
                new CertificationGroupRetriesOffsetAdjuster(0.1),
                new MinimumCertificationGroupAdjuster(29.8),
                new NameCertificationGroupAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.21, ["retail"] = 1.02 }),
                new RoundingCertificationGroupAdjuster(0.5),
                new PercentCertificationGroupAdjuster(7.4),
                new CapCertificationGroupAdjuster(5848.0),
            },
            new List<string>(),
            1.3,
            71758.4);
    }

    public double Price(CertificationGroup item)
    {
        var current = item.NetLoad();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<CertificationGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
