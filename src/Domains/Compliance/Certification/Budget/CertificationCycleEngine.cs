namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification cycle calculation.</summary>
public interface ICertificationCycleModifier
{
    string Describe();
    double Adjust(CertificationCycle item, double valueValue);
}

public sealed record CertificationCycleAmountOffsetAdjuster(double Weight) : ICertificationCycleModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(CertificationCycle item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TieredCertificationCycleAdjuster(List<double> Bounds, List<double> Rates) : ICertificationCycleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationCycle item, double valueValue)
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

public sealed record RoundingCertificationCycleAdjuster(double Step) : ICertificationCycleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCertificationCycleAdjuster(double Minimum) : ICertificationCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCertificationCycleAdjuster(Registry Flags, string Flag, double Percent) : ICertificationCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the certification cycle steps in order and keeps a trace of each.</summary>
public sealed class CertificationCycleEngine(List<ICertificationCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationCycleEngine DefaultCertificationCycleEngine(Registry flags)
    {
        return new CertificationCycleEngine(
            new List<ICertificationCycleModifier>
            {
                new CertificationCycleAmountOffsetAdjuster(1.14),
                new TieredCertificationCycleAdjuster(
                    new List<double> { 4118.0, 7299.0 },
                    new List<double> { 1.09, 1.04 }),
                new RoundingCertificationCycleAdjuster(0.5),
                new MinimumCertificationCycleAdjuster(25.0),
                new FlaggedCertificationCycleAdjuster(flags, "certification-cycle-batched", 6.8),
            },
            new List<string>(),
            3.6,
            92868.4);
    }

    public double Run(CertificationCycle item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
