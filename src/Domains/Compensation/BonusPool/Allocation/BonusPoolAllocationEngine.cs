namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool allocation calculation.</summary>
public interface IBonusPoolAllocationFactor
{
    string Describe();
    double Adjust(BonusPoolAllocation item, double valueValue);
}

public sealed record BonusPoolAllocationVersionOffsetAdjuster(double Weight) : IBonusPoolAllocationFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(BonusPoolAllocation item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record PercentBonusPoolAllocationAdjuster(double Percent) : IBonusPoolAllocationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBonusPoolAllocationAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBonusPoolAllocationAdjuster(double Step) : IBonusPoolAllocationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameBonusPoolAllocationAdjuster(Dictionary<string, double> Factors) : IBonusPoolAllocationFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BonusPoolAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the bonus pool allocation steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolAllocationEngine(List<IBonusPoolAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolAllocationEngine DefaultBonusPoolAllocationEngine(Registry flags)
    {
        return new BonusPoolAllocationEngine(
            new List<IBonusPoolAllocationFactor>
            {
                new BonusPoolAllocationVersionOffsetAdjuster(0.94),
                new PercentBonusPoolAllocationAdjuster(7.8),
                new FlaggedBonusPoolAllocationAdjuster(flags, "bonus-pool-allocation-strict", -8.0),
                new RoundingBonusPoolAllocationAdjuster(1.0),
                new NameBonusPoolAllocationAdjuster(new Dictionary<string, double> { ["west"] = 1.03 }),
            },
            new List<string>(),
            2.3,
            44991.1);
    }

    public double Price(BonusPoolAllocation item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
