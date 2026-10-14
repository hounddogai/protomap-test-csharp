namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy certification digest calculation.</summary>
public interface ILegacyCertificationDigestModifier
{
    string Describe();
    double Adjust(LegacyCertificationDigest item, double valueValue);
}

public sealed record LegacyCertificationDigestAttemptsOffsetAdjuster(double Weight) : ILegacyCertificationDigestModifier
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(LegacyCertificationDigest item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record TieredLegacyCertificationDigestAdjuster(List<double> Bounds, List<double> Rates) : ILegacyCertificationDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyCertificationDigest item, double valueValue)
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

public sealed record PercentLegacyCertificationDigestAdjuster(double Percent) : ILegacyCertificationDigestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyCertificationDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLegacyCertificationDigestAdjuster(Registry Flags, string Flag, double Percent) : ILegacyCertificationDigestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyCertificationDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyCertificationDigestAdjuster(double Minimum) : ILegacyCertificationDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyCertificationDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLegacyCertificationDigestAdjuster(double Cap) : ILegacyCertificationDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyCertificationDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy certification digest steps in order and keeps a trace of each.</summary>
public sealed class LegacyCertificationDigestEngine(List<ILegacyCertificationDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyCertificationDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyCertificationDigestEngine DefaultLegacyCertificationDigestEngine(Registry flags)
    {
        return new LegacyCertificationDigestEngine(
            new List<ILegacyCertificationDigestModifier>
            {
                new LegacyCertificationDigestAttemptsOffsetAdjuster(1.35),
                new TieredLegacyCertificationDigestAdjuster(
                    new List<double> { 1760.0, 4541.0, 7255.0 },
                    new List<double> { 0.91, 0.98, 0.82 }),
                new PercentLegacyCertificationDigestAdjuster(7.1),
                new FlaggedLegacyCertificationDigestAdjuster(flags, "legacy-certification-digest-weighted", -9.9),
                new MinimumLegacyCertificationDigestAdjuster(30.9),
                new CapLegacyCertificationDigestAdjuster(21532.0),
            },
            new List<string>(),
            2.3,
            63393.3);
    }

    public double Run(LegacyCertificationDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyCertificationDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
