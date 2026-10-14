namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification proposal calculation.</summary>
public interface ICertificationProposalFactor
{
    string Describe();
    double Adjust(CertificationProposal item, double valueValue);
}

public sealed record RoundingCertificationProposalAdjuster(double Step) : ICertificationProposalFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameCertificationProposalAdjuster(Dictionary<string, double> Factors) : ICertificationProposalFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(CertificationProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CertificationProposalDurationMinutesOffsetAdjuster(double Weight) : ICertificationProposalFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(CertificationProposal item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record CapCertificationProposalAdjuster(double Cap) : ICertificationProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentCertificationProposalAdjuster(double Percent) : ICertificationProposalFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCertificationProposalAdjuster(double Minimum) : ICertificationProposalFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the certification proposal steps in order and keeps a trace of each.</summary>
public sealed class CertificationProposalEngine(List<ICertificationProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationProposalEngine DefaultCertificationProposalEngine()
    {
        return new CertificationProposalEngine(
            new List<ICertificationProposalFactor>
            {
                new RoundingCertificationProposalAdjuster(1.0),
                new NameCertificationProposalAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.85, ["central"] = 1.28, ["online"] = 1.31 }),
                new CertificationProposalDurationMinutesOffsetAdjuster(1.12),
                new CapCertificationProposalAdjuster(34741.0),
                new PercentCertificationProposalAdjuster(-13.4),
                new MinimumCertificationProposalAdjuster(28.9),
            },
            new List<string>(),
            5.0,
            58034.2);
    }

    public double Evaluate(CertificationProposal item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
