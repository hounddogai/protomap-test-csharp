namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped certification assignment calculation.</summary>
public interface ICappedCertificationAssignmentModifier
{
    string Describe();
    double Adjust(CappedCertificationAssignment item, double valueValue);
}

public sealed record CappedCertificationAssignmentRetriesOffsetAdjuster(double Weight) : ICappedCertificationAssignmentModifier
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(CappedCertificationAssignment item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record FlaggedCappedCertificationAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ICappedCertificationAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedCertificationAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCappedCertificationAssignmentAdjuster(double Minimum) : ICappedCertificationAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedCertificationAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentCappedCertificationAssignmentAdjuster(double Percent) : ICappedCertificationAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedCertificationAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedCertificationAssignmentAdjuster(List<double> Bounds, List<double> Rates) : ICappedCertificationAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedCertificationAssignment item, double valueValue)
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

/// <summary>Runs the capped certification assignment steps in order and keeps a trace of each.</summary>
public sealed class CappedCertificationAssignmentEngine(List<ICappedCertificationAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedCertificationAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedCertificationAssignmentEngine DefaultCappedCertificationAssignmentEngine(Registry flags)
    {
        return new CappedCertificationAssignmentEngine(
            new List<ICappedCertificationAssignmentModifier>
            {
                new CappedCertificationAssignmentRetriesOffsetAdjuster(1.4),
                new FlaggedCappedCertificationAssignmentAdjuster(
                    flags,
                    "capped-certification-assignment-default",
                    8.5),
                new MinimumCappedCertificationAssignmentAdjuster(20.8),
                new PercentCappedCertificationAssignmentAdjuster(-8.7),
                new TieredCappedCertificationAssignmentAdjuster(
                    new List<double> { 2220.0, 6395.0, 9354.0 },
                    new List<double> { 0.96, 0.85, 0.88 }),
            },
            new List<string>(),
            4.1,
            67719.5);
    }

    public double Price(CappedCertificationAssignment item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedCertificationAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
