namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool cycle calculation.</summary>
public interface IBonusPoolCycleModifier
{
    string Describe();
    double Adjust(BonusPoolCycle item, double valueValue);
}

public sealed record RoundingBonusPoolCycleAdjuster(double Step) : IBonusPoolCycleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBonusPoolCycleAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBonusPoolCycleAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolCycleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolCycle item, double valueValue)
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

public sealed record MinimumBonusPoolCycleAdjuster(double Minimum) : IBonusPoolCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBonusPoolCycleAdjuster(double Cap) : IBonusPoolCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BonusPoolCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the bonus pool cycle steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolCycleEngine(List<IBonusPoolCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolCycleEngine DefaultBonusPoolCycleEngine(Registry flags)
    {
        return new BonusPoolCycleEngine(
            new List<IBonusPoolCycleModifier>
            {
                new RoundingBonusPoolCycleAdjuster(0.5),
                new FlaggedBonusPoolCycleAdjuster(flags, "bonus-pool-cycle-weighted", -4.7),
                new TieredBonusPoolCycleAdjuster(
                    new List<double> { 1609.0, 7107.0 },
                    new List<double> { 1.18, 1.08 }),
                new MinimumBonusPoolCycleAdjuster(2.9),
                new CapBonusPoolCycleAdjuster(44506.0),
            },
            new List<string>(),
            3.4,
            31367.4);
    }

    public double Compute(BonusPoolCycle item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
