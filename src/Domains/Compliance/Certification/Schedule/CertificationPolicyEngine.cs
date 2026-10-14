namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification policy calculation.</summary>
public interface ICertificationPolicyFactor
{
    string Describe();
    double Adjust(CertificationPolicy item, double valueValue);
}

public sealed record FlaggedCertificationPolicyAdjuster(Registry Flags, string Flag, double Percent) : ICertificationPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCertificationPolicyAdjuster(double Percent) : ICertificationPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCertificationPolicyAdjuster(double Cap) : ICertificationPolicyFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CertificationPolicyVersionOffsetAdjuster(double Weight) : ICertificationPolicyFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(CertificationPolicy item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record RoundingCertificationPolicyAdjuster(double Step) : ICertificationPolicyFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the certification policy steps in order and keeps a trace of each.</summary>
public sealed class CertificationPolicyEngine(List<ICertificationPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationPolicyEngine DefaultCertificationPolicyEngine(Registry flags)
    {
        return new CertificationPolicyEngine(
            new List<ICertificationPolicyFactor>
            {
                new FlaggedCertificationPolicyAdjuster(flags, "certification-policy-pending", -9.1),
                new PercentCertificationPolicyAdjuster(-23.0),
                new CapCertificationPolicyAdjuster(5627.0),
                new CertificationPolicyVersionOffsetAdjuster(0.94),
                new RoundingCertificationPolicyAdjuster(0.05),
            },
            new List<string>(),
            1.9,
            69600.1);
    }

    public double Run(CertificationPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<CertificationPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
