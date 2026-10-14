namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped offboarding slot calculation.</summary>
public interface ICappedOffboardingSlotStep
{
    string Describe();
    double Adjust(CappedOffboardingSlot item, double valueValue);
}

public sealed record MinimumCappedOffboardingSlotAdjuster(double Minimum) : ICappedOffboardingSlotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedOffboardingSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CappedOffboardingSlotExpiresAtOffsetAdjuster(double Weight) : ICappedOffboardingSlotStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(CappedOffboardingSlot item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record TieredCappedOffboardingSlotAdjuster(List<double> Bounds, List<double> Rates) : ICappedOffboardingSlotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedOffboardingSlot item, double valueValue)
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

public sealed record FlaggedCappedOffboardingSlotAdjuster(Registry Flags, string Flag, double Percent) : ICappedOffboardingSlotStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedOffboardingSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleCappedOffboardingSlotAdjuster(Dictionary<string, double> Factors) : ICappedOffboardingSlotStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(CappedOffboardingSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingCappedOffboardingSlotAdjuster(double Step) : ICappedOffboardingSlotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedOffboardingSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped offboarding slot steps in order and keeps a trace of each.</summary>
public sealed class CappedOffboardingSlotEngine(List<ICappedOffboardingSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedOffboardingSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedOffboardingSlotEngine DefaultCappedOffboardingSlotEngine(Registry flags)
    {
        return new CappedOffboardingSlotEngine(
            new List<ICappedOffboardingSlotStep>
            {
                new MinimumCappedOffboardingSlotAdjuster(39.6),
                new CappedOffboardingSlotExpiresAtOffsetAdjuster(1.69),
                new TieredCappedOffboardingSlotAdjuster(
                    new List<double> { 2040.0, 5693.0, 7308.0 },
                    new List<double> { 0.88, 1.17, 0.83 }),
                new FlaggedCappedOffboardingSlotAdjuster(flags, "capped-offboarding-slot-fallback", -4.7),
                new LocaleCappedOffboardingSlotAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.34, ["retail"] = 0.74 }),
                new RoundingCappedOffboardingSlotAdjuster(1.0),
            },
            new List<string>(),
            4.8,
            30569.4);
    }

    public double Compute(CappedOffboardingSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedOffboardingSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
