namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification target calculation.</summary>
public interface ICertificationTargetStep
{
    string Describe();
    double Adjust(CertificationTarget item, double valueValue);
}

public sealed record MinimumCertificationTargetAdjuster(double Minimum) : ICertificationTargetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCertificationTargetAdjuster(Registry Flags, string Flag, double Percent) : ICertificationTargetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCertificationTargetAdjuster(double Percent) : ICertificationTargetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCertificationTargetAdjuster(List<double> Bounds, List<double> Rates) : ICertificationTargetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationTarget item, double valueValue)
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

public sealed record RoundingCertificationTargetAdjuster(double Step) : ICertificationTargetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitCertificationTargetAdjuster(Dictionary<string, double> Factors) : ICertificationTargetStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(CertificationTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the certification target steps in order and keeps a trace of each.</summary>
public sealed class CertificationTargetEngine(List<ICertificationTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationTargetEngine DefaultCertificationTargetEngine(Registry flags)
    {
        return new CertificationTargetEngine(
            new List<ICertificationTargetStep>
            {
                new MinimumCertificationTargetAdjuster(33.9),
                new FlaggedCertificationTargetAdjuster(flags, "certification-target-global", -0.3),
                new PercentCertificationTargetAdjuster(-4.5),
                new TieredCertificationTargetAdjuster(
                    new List<double> { 3974.0, 4580.0, 4583.0, 8610.0 },
                    new List<double> { 1.08, 0.88, 0.84, 0.99 }),
                new RoundingCertificationTargetAdjuster(1.0),
                new UnitCertificationTargetAdjuster(new Dictionary<string, double> { ["north"] = 1.2 }),
            },
            new List<string>(),
            0.3,
            48393.8);
    }

    public double Evaluate(CertificationTarget item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
