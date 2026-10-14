namespace Acme.Workforce.Orgdesign.Reorg.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg baseline calculation.</summary>
public interface IReorgBaselineAdjuster
{
    string Describe();
    double Adjust(ReorgBaseline item, double valueValue);
}

public sealed record RoundingReorgBaselineAdjuster(double Step) : IReorgBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumReorgBaselineAdjuster(double Minimum) : IReorgBaselineAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ReorgBaselineRetriesOffsetAdjuster(double Weight) : IReorgBaselineAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(ReorgBaseline item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record CapReorgBaselineAdjuster(double Cap) : IReorgBaselineAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReorgBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedReorgBaselineAdjuster(Registry Flags, string Flag, double Percent) : IReorgBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReorgBaselineAdjuster(List<double> Bounds, List<double> Rates) : IReorgBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgBaseline item, double valueValue)
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

/// <summary>Runs the reorg baseline steps in order and keeps a trace of each.</summary>
public sealed class ReorgBaselineEngine(List<IReorgBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgBaselineEngine DefaultReorgBaselineEngine(Registry flags)
    {
        return new ReorgBaselineEngine(
            new List<IReorgBaselineAdjuster>
            {
                new RoundingReorgBaselineAdjuster(0.5),
                new MinimumReorgBaselineAdjuster(0.7),
                new ReorgBaselineRetriesOffsetAdjuster(1.21),
                new CapReorgBaselineAdjuster(41732.0),
                new FlaggedReorgBaselineAdjuster(flags, "reorg-baseline-scheduled", -7.1),
                new TieredReorgBaselineAdjuster(
                    new List<double> { 3424.0, 4704.0, 6531.0 },
                    new List<double> { 1.06, 1.12, 0.95 }),
            },
            new List<string>(),
            3.1,
            55799.5);
    }

    public double Price(ReorgBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<ReorgBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
