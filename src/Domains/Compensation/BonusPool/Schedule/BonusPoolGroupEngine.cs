namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool group calculation.</summary>
public interface IBonusPoolGroupModifier
{
    string Describe();
    double Adjust(BonusPoolGroup item, double valueValue);
}

public sealed record BonusPoolGroupRateOffsetAdjuster(double Weight) : IBonusPoolGroupModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(BonusPoolGroup item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record FlaggedBonusPoolGroupAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBonusPoolGroupAdjuster(double Cap) : IBonusPoolGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BonusPoolGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBonusPoolGroupAdjuster(double Step) : IBonusPoolGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the bonus pool group steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolGroupEngine(List<IBonusPoolGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolGroupEngine DefaultBonusPoolGroupEngine(Registry flags)
    {
        return new BonusPoolGroupEngine(
            new List<IBonusPoolGroupModifier>
            {
                new BonusPoolGroupRateOffsetAdjuster(1.63),
                new FlaggedBonusPoolGroupAdjuster(flags, "bonus-pool-group-lenient", 5.7),
                new CapBonusPoolGroupAdjuster(17151.0),
                new RoundingBonusPoolGroupAdjuster(0.25),
            },
            new List<string>(),
            3.1,
            59213.5);
    }

    public double Run(BonusPoolGroup item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
