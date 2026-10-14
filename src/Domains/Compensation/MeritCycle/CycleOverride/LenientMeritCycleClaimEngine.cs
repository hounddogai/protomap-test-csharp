namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient merit cycle claim calculation.</summary>
public interface ILenientMeritCycleClaimModifier
{
    string Describe();
    double Adjust(LenientMeritCycleClaim item, double valueValue);
}

public sealed record FlaggedLenientMeritCycleClaimAdjuster(Registry Flags, string Flag, double Percent) : ILenientMeritCycleClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientMeritCycleClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientMeritCycleClaimAdjuster(List<double> Bounds, List<double> Rates) : ILenientMeritCycleClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientMeritCycleClaim item, double valueValue)
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

public sealed record PercentLenientMeritCycleClaimAdjuster(double Percent) : ILenientMeritCycleClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientMeritCycleClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelLenientMeritCycleClaimAdjuster(Dictionary<string, double> Factors) : ILenientMeritCycleClaimModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LenientMeritCycleClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLenientMeritCycleClaimAdjuster(double Step) : ILenientMeritCycleClaimModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientMeritCycleClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient merit cycle claim steps in order and keeps a trace of each.</summary>
public sealed class LenientMeritCycleClaimEngine(List<ILenientMeritCycleClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientMeritCycleClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientMeritCycleClaimEngine DefaultLenientMeritCycleClaimEngine(Registry flags)
    {
        return new LenientMeritCycleClaimEngine(
            new List<ILenientMeritCycleClaimModifier>
            {
                new FlaggedLenientMeritCycleClaimAdjuster(flags, "lenient-merit-cycle-claim-weighted", -7.1),
                new TieredLenientMeritCycleClaimAdjuster(
                    new List<double> { 4768.0, 8828.0 },
                    new List<double> { 1.06, 0.82 }),
                new PercentLenientMeritCycleClaimAdjuster(4.2),
                new ChannelLenientMeritCycleClaimAdjuster(new Dictionary<string, double> { ["north"] = 0.84 }),
                new RoundingLenientMeritCycleClaimAdjuster(0.05),
            },
            new List<string>(),
            3.5,
            57574.4);
    }

    public double Run(LenientMeritCycleClaim item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientMeritCycleClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
