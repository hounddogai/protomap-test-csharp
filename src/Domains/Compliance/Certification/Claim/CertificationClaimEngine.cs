namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the certification claim calculation.</summary>
public interface ICertificationClaimStep
{
    string Describe();
    double Adjust(CertificationClaim item, double valueValue);
}

public sealed record CapCertificationClaimAdjuster(double Cap) : ICertificationClaimStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelCertificationClaimAdjuster(Dictionary<string, double> Factors) : ICertificationClaimStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CertificationClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentCertificationClaimAdjuster(double Percent) : ICertificationClaimStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCertificationClaimAdjuster(Registry Flags, string Flag, double Percent) : ICertificationClaimStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the certification claim steps in order and keeps a trace of each.</summary>
public sealed class CertificationClaimEngine(List<ICertificationClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationClaimEngine DefaultCertificationClaimEngine(Registry flags)
    {
        return new CertificationClaimEngine(
            new List<ICertificationClaimStep>
            {
                new CapCertificationClaimAdjuster(27410.0),
                new ChannelCertificationClaimAdjuster(new Dictionary<string, double> { ["north"] = 1.01 }),
                new PercentCertificationClaimAdjuster(10.9),
                new FlaggedCertificationClaimAdjuster(flags, "certification-claim-legacy", 3.2),
            },
            new List<string>(),
            1.2,
            68958.8);
    }

    public double Price(CertificationClaim item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
