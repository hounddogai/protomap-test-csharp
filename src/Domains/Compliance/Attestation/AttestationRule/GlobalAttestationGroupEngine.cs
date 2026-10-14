namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global attestation group calculation.</summary>
public interface IGlobalAttestationGroupModifier
{
    string Describe();
    double Adjust(GlobalAttestationGroup item, double valueValue);
}

public sealed record TieredGlobalAttestationGroupAdjuster(List<double> Bounds, List<double> Rates) : IGlobalAttestationGroupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalAttestationGroup item, double valueValue)
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

public sealed record MinimumGlobalAttestationGroupAdjuster(double Minimum) : IGlobalAttestationGroupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalAttestationGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedGlobalAttestationGroupAdjuster(Registry Flags, string Flag, double Percent) : IGlobalAttestationGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalAttestationGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentGlobalAttestationGroupAdjuster(double Percent) : IGlobalAttestationGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalAttestationGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingGlobalAttestationGroupAdjuster(double Step) : IGlobalAttestationGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalAttestationGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global attestation group steps in order and keeps a trace of each.</summary>
public sealed class GlobalAttestationGroupEngine(List<IGlobalAttestationGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAttestationGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAttestationGroupEngine DefaultGlobalAttestationGroupEngine(Registry flags)
    {
        return new GlobalAttestationGroupEngine(
            new List<IGlobalAttestationGroupModifier>
            {
                new TieredGlobalAttestationGroupAdjuster(
                    new List<double> { 7787.0, 9310.0, 9590.0 },
                    new List<double> { 0.97, 1.03, 0.99 }),
                new MinimumGlobalAttestationGroupAdjuster(0.0),
                new FlaggedGlobalAttestationGroupAdjuster(flags, "global-attestation-group-global", -6.6),
                new PercentGlobalAttestationGroupAdjuster(-4.5),
                new RoundingGlobalAttestationGroupAdjuster(0.5),
            },
            new List<string>(),
            3.8,
            38744.5);
    }

    public double Price(GlobalAttestationGroup item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAttestationGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
