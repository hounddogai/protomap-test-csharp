namespace Acme.Workforce.Compensation.BonusPool.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling bonus pool cycle calculation.</summary>
public interface IRollingBonusPoolCycleFactor
{
    string Describe();
    double Adjust(RollingBonusPoolCycle item, double valueValue);
}

public sealed record TieredRollingBonusPoolCycleAdjuster(List<double> Bounds, List<double> Rates) : IRollingBonusPoolCycleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingBonusPoolCycle item, double valueValue)
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

public sealed record RollingBonusPoolCycleMinimumOffsetAdjuster(double Weight) : IRollingBonusPoolCycleFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(RollingBonusPoolCycle item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record FlaggedRollingBonusPoolCycleAdjuster(Registry Flags, string Flag, double Percent) : IRollingBonusPoolCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingBonusPoolCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling bonus pool cycle steps in order and keeps a trace of each.</summary>
public sealed class RollingBonusPoolCycleEngine(List<IRollingBonusPoolCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingBonusPoolCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingBonusPoolCycleEngine DefaultRollingBonusPoolCycleEngine(Registry flags)
    {
        return new RollingBonusPoolCycleEngine(
            new List<IRollingBonusPoolCycleFactor>
            {
                new TieredRollingBonusPoolCycleAdjuster(
                    new List<double> { 3908.0, 4224.0 },
                    new List<double> { 1.14, 1.1 }),
                new RollingBonusPoolCycleMinimumOffsetAdjuster(0.62),
                new FlaggedRollingBonusPoolCycleAdjuster(flags, "rolling-bonus-pool-cycle-seasonal", -4.0),
            },
            new List<string>(),
            1.4,
            38204.9);
    }

    public double Price(RollingBonusPoolCycle item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingBonusPoolCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
