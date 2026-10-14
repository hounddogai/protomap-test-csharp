namespace Acme.Workforce.Compliance.Certification.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual certification claim calculation.</summary>
public interface IManualCertificationClaimModifier
{
    string Describe();
    double Adjust(ManualCertificationClaim item, double valueValue);
}

public sealed record NameManualCertificationClaimAdjuster(Dictionary<string, double> Factors) : IManualCertificationClaimModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ManualCertificationClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedManualCertificationClaimAdjuster(Registry Flags, string Flag, double Percent) : IManualCertificationClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualCertificationClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualCertificationClaimAdjuster(double Step) : IManualCertificationClaimModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualCertificationClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumManualCertificationClaimAdjuster(double Minimum) : IManualCertificationClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualCertificationClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredManualCertificationClaimAdjuster(List<double> Bounds, List<double> Rates) : IManualCertificationClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualCertificationClaim item, double valueValue)
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

public sealed record ManualCertificationClaimEndsAtOffsetAdjuster(double Weight) : IManualCertificationClaimModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(ManualCertificationClaim item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the manual certification claim steps in order and keeps a trace of each.</summary>
public sealed class ManualCertificationClaimEngine(List<IManualCertificationClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualCertificationClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualCertificationClaimEngine DefaultManualCertificationClaimEngine(Registry flags)
    {
        return new ManualCertificationClaimEngine(
            new List<IManualCertificationClaimModifier>
            {
                new NameManualCertificationClaimAdjuster(new Dictionary<string, double> { ["south"] = 0.72 }),
                new FlaggedManualCertificationClaimAdjuster(flags, "manual-certification-claim-regional", -8.1),
                new RoundingManualCertificationClaimAdjuster(0.05),
                new MinimumManualCertificationClaimAdjuster(27.2),
                new TieredManualCertificationClaimAdjuster(
                    new List<double> { 1844.0, 8938.0 },
                    new List<double> { 0.92, 0.83 }),
                new ManualCertificationClaimEndsAtOffsetAdjuster(1.15),
            },
            new List<string>(),
            2.3,
            9378.1);
    }

    public double Compute(ManualCertificationClaim item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualCertificationClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
